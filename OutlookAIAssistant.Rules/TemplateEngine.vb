Namespace OutlookAIAssistant.Rules

    ''' <summary>
    ''' Template engine for generating reply templates and email content.
    ''' Uses placeholders like {{sender_name}}, {{subject}}, etc.
    ''' </summary>
    Public Class TemplateEngine

        ''' <summary>
        ''' Renders a template by replacing placeholders with actual values.
        ''' </summary>
        Public Function Render(template As String, parameters As Dictionary(Of String, String)) As String
            Dim result As String = template
            For Each kvp In parameters
                result = result.Replace($"{{{{{kvp.Key}}}}}", kvp.Value)
            Next
            Return result
        End Function

        ''' <summary>
        ''' Applies a support template to an incoming support request email.
        ''' </summary>
        Public Function ApplySupportTemplate(templateName As String, emailSubject As String, emailBody As String) As String
            Dim template = GetTemplate(templateName)
            Dim parameters = New Dictionary(Of String, String) From {
                {"subject", emailSubject},
                {"body", emailBody},
                {"date", DateTime.Now.ToString("yyyy-MM-dd")}
            }
            Return Render(template, parameters)
        End Function

        Private Function GetTemplate(templateName As String) As String
            ' In production, load from database or file
            Select Case templateName.ToLowerInvariant()
                Case "support_acknowledgment"
                    Return "Thank you for contacting support regarding '{{subject}}'. We have received your request and will respond within 24 hours."
                Case "support_resolved"
                    Return "Your support request regarding '{{subject}}' has been resolved. Please let us know if you have any further questions."
                Case Else
                    Return "Template '{{templateName}}' not found."
            End Select
        End Function

    End Class

End Namespace