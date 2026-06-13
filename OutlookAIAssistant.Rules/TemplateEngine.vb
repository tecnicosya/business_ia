Imports System.Text.RegularExpressions
Imports OutlookAIAssistant.Core.Models

Namespace OutlookAIAssistant.Rules

    ''' <summary>
    ''' Template engine for rendering email templates with variable substitution,
    ''' conditional blocks, and template repository management.
    ''' 
    ''' Variables: {{name}}, {{company}}, {{email}}, {{date}}, {{subject}}, {{body}},
    ''' {{classification}}, {{sentiment}}, {{priority}}, {{sender_name}}, {{sender_email}},
    ''' {{recipient_name}}, {{recipient_email}}, {{year}}, {{time}}
    ''' 
    ''' Conditionals: {{#if variable}}...{{/if}}, {{#unless variable}}...{{/unless}},
    ''' {{#eq variable value}}...{{/eq}}
    ''' </summary>
    Public Class TemplateEngine

        Private ReadOnly _templates As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

        ''' <summary>
        ''' Initializes the template engine with built-in default templates.
        ''' </summary>
        Public Sub New()
            LoadDefaultTemplates()
        End Sub

        ''' <summary>
        ''' Registers a template by name. Overwrites any existing template with the same name.
        ''' </summary>
        Public Sub RegisterTemplate(name As String, content As String)
            _templates(name) = content
        End Sub

        ''' <summary>
        ''' Removes a template by name.
        ''' </summary>
        Public Sub UnregisterTemplate(name As String)
            If _templates.ContainsKey(name) Then
                _templates.Remove(name)
            End If
        End Sub

        ''' <summary>
        ''' Gets a registered template by name. Returns Nothing if not found.
        ''' </summary>
        Public Function GetTemplate(name As String) As String
            If _templates.TryGetValue(name, Dim template) Then
                Return template
            End If
            Return Nothing
        End Function

        ''' <summary>
        ''' Returns all registered template names.
        ''' </summary>
        Public Function GetTemplateNames() As IReadOnlyList(Of String)
            Return _templates.Keys.ToList().AsReadOnly()
        End Function

        ''' <summary>
        ''' Renders a template by replacing all variables and processing conditional blocks.
        ''' </summary>
        Public Function Render(template As String, parameters As Dictionary(Of String, String)) As String
            If String.IsNullOrEmpty(template) Then Return String.Empty
            If parameters Is Nothing Then parameters = New Dictionary(Of String, String)()

            Dim result As String = template

            ' 1. Process conditional blocks first (#if, #unless, #eq)
            result = ProcessConditionals(result, parameters)

            ' 2. Replace all variable placeholders
            For Each kvp In parameters
                Dim placeholder = $"{{{{{kvp.Key}}}}}"
                result = result.Replace(placeholder, If(kvp.Value, ""))
            Next

            ' 3. Add default variables that weren't in parameters
            If Not parameters.ContainsKey("date") Then
                result = result.Replace("{{date}}", DateTime.Now.ToString("yyyy-MM-dd"))
            End If
            If Not parameters.ContainsKey("year") Then
                result = result.Replace("{{year}}", DateTime.Now.Year.ToString())
            End If
            If Not parameters.ContainsKey("time") Then
                result = result.Replace("{{time}}", DateTime.Now.ToString("HH:mm"))
            End If

            ' 4. Clean up any remaining unreplaced variables
            result = Regex.Replace(result, "\{\{[^}]+\}\}", "")

            Return result
        End Function

        ''' <summary>
        ''' Renders a named template with the given parameters.
        ''' Throws KeyNotFoundException if the template doesn't exist.
        ''' </summary>
        Public Function RenderNamed(name As String, parameters As Dictionary(Of String, String)) As String
            Dim template = GetTemplate(name)
            If template Is Nothing Then
                Throw New KeyNotFoundException($"Template '{name}' not found.")
            End If
            Return Render(template, parameters)
        End Function

        ''' <summary>
        ''' Applies a support template to an incoming email. Convenience wrapper
        ''' that builds parameters from email data.
        ''' </summary>
        Public Function ApplySupportTemplate(templateName As String, email As EmailMessage) As String
            Dim parameters = BuildEmailParameters(email)
            Return RenderNamed(templateName, parameters)
        End Function

        ''' <summary>
        ''' Builds template parameters from an EmailMessage object.
        ''' </summary>
        Public Function BuildEmailParameters(email As EmailMessage) As Dictionary(Of String, String)
            Dim parameters = New Dictionary(Of String, String) From {
                {"subject", If(email.Subject, "")},
                {"body", If(email.Body, "")},
                {"sender_name", If(email.SenderName, "")},
                {"sender_email", If(email.SenderEmail, "")},
                {"date", DateTime.Now.ToString("yyyy-MM-dd")},
                {"time", DateTime.Now.ToString("HH:mm")},
                {"year", DateTime.Now.Year.ToString()},
                {"recipient_name", ""},
                {"recipient_email", ""}
            }

            If email.Classification IsNot Nothing Then
                parameters("classification") = If(email.Classification.Type.ToString(), "")
                parameters("subcategory") = If(email.Classification.SubCategory, "")
                parameters("sentiment") = If(email.Classification.Sentiment, "")
                parameters("priority") = If(email.Classification.Priority, "")
                parameters("confidence") = email.Classification.Confidence.ToString("P0")
            End If

            Return parameters
        End Function

        ''' <summary>
        ''' Processes conditional blocks in a template: {{#if var}}...{{/if}},
        ''' {{#unless var}}...{{/unless}}, {{#eq var value}}...{{/eq}}.
        ''' Supports nested blocks.
        ''' </summary>
        Private Function ProcessConditionals(template As String, parameters As Dictionary(Of String, String)) As String
            Dim result As String = template

            ' Process #eq blocks: {{#eq var value}}content{{/eq}}
            result = Regex.Replace(result,
                "\{\{#eq\s+(\w+)\s+(.+?)\}\}(.*?)\{\{/eq\}\}",
                Function(m As Match) As String
                    Dim varName = m.Groups(1).Value
                    Dim expectedValue = m.Groups(2).Value.Trim()
                    Dim content = m.Groups(3).Value
                    Dim actualValue = If(parameters.ContainsKey(varName), parameters(varName), "")
                    If String.Equals(actualValue, expectedValue, StringComparison.OrdinalIgnoreCase) Then
                        Return content
                    End If
                    Return ""
                End Function,
                RegexOptions.Singleline Or RegexOptions.IgnoreCase)

            ' Process #if blocks: {{#if var}}content{{/if}}
            result = Regex.Replace(result,
                "\{\{#if\s+(\w+)\}\}(.*?)\{\{/if\}\}",
                Function(m As Match) As String
                    Dim varName = m.Groups(1).Value
                    Dim content = m.Groups(2).Value
                    Dim actualValue = If(parameters.ContainsKey(varName), parameters(varName), "")
                    If Not String.IsNullOrEmpty(actualValue) AndAlso
                       Not String.Equals(actualValue, "false", StringComparison.OrdinalIgnoreCase) AndAlso
                       Not String.Equals(actualValue, "0", StringComparison.OrdinalIgnoreCase) Then
                        Return content
                    End If
                    Return ""
                End Function,
                RegexOptions.Singleline Or RegexOptions.IgnoreCase)

            ' Process #unless blocks: {{#unless var}}content{{/unless}}
            result = Regex.Replace(result,
                "\{\{#unless\s+(\w+)\}\}(.*?)\{\{/unless\}\}",
                Function(m As Match) As String
                    Dim varName = m.Groups(1).Value
                    Dim content = m.Groups(2).Value
                    Dim actualValue = If(parameters.ContainsKey(varName), parameters(varName), "")
                    If String.IsNullOrEmpty(actualValue) OrElse
                       String.Equals(actualValue, "false", StringComparison.OrdinalIgnoreCase) OrElse
                       String.Equals(actualValue, "0", StringComparison.OrdinalIgnoreCase) Then
                        Return content
                    End If
                    Return ""
                End Function,
                RegexOptions.Singleline Or RegexOptions.IgnoreCase)

            Return result
        End Function

        ''' <summary>
        ''' Loads the built-in default templates.
        ''' </summary>
        Private Sub LoadDefaultTemplates()
            _templates("support_acknowledgment") =
                "Thank you for contacting support regarding '{{subject}}'. " &
                "We have received your request and will respond within 24 hours."

            _templates("support_resolved") =
                "Your support request regarding '{{subject}}' has been resolved. " &
                "Please let us know if you have any further questions."

            _templates("support_followup") =
                "Regarding your request '{{subject}}', we need some additional information. " &
                "{{#if details}}{{details}}{{/if}}"

            _templates("sales_followup") =
                "Hi {{sender_name}}," & vbCrLf &
                vbCrLf &
                "Thank you for your interest in our services. I wanted to follow up " &
                "on our previous conversation regarding '{{subject}}'." & vbCrLf &
                vbCrLf &
                "Please let me know if you have any questions or would like to schedule a call." &
                vbCrLf &
                vbCrLf &
                "Best regards," & vbCrLf &
                "{{recipient_name}}" & vbCrLf &
                "{{sender_email}}"

            _templates("general_acknowledgment") =
                "Hi {{sender_name}}," & vbCrLf &
                vbCrLf &
                "Thank you for your message regarding '{{subject}}'. " &
                "I will review it and get back to you shortly." & vbCrLf &
                vbCrLf &
                "Best regards," & vbCrLf &
                "{{recipient_name}}"

            _templates("meeting_confirmation") =
                "Hi {{sender_name}}," & vbCrLf &
                vbCrLf &
                "Thank you for the meeting request regarding '{{subject}}'. " &
                "I have confirmed the appointment and look forward to our discussion." & vbCrLf &
                vbCrLf &
                "Best regards," & vbCrLf &
                "{{recipient_name}}"

            _templates("out_of_office") =
                "Thank you for your email." & vbCrLf &
                vbCrLf &
                "I am currently out of the office and will have limited access to email " &
                "until {{date}}. If you need immediate assistance, please contact " &
                "our support team." & vbCrLf &
                vbCrLf &
                "I will respond to your message as soon as possible upon my return." &
                vbCrLf &
                vbCrLf &
                "Best regards," & vbCrLf &
                "{{recipient_name}}"
        End Sub

    End Class

End Namespace