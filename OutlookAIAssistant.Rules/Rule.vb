Imports System.Threading.Tasks
Imports OutlookAIAssistant.Core.Models

Namespace OutlookAIAssistant.Rules

    ''' <summary>
    ''' Helper utilities for working with Rule, RuleCondition, and RuleAction objects.
    ''' Provides validation, cloning, and serialization support.
    ''' </summary>
    Public Module RuleHelper

        ''' <summary>
        ''' Validates a rule's structure and returns a list of validation errors.
        ''' Returns an empty list if the rule is valid.
        ''' </summary>
        Public Function ValidateRule(rule As Rule) As List(Of String)
            Dim errors As New List(Of String)()

            If String.IsNullOrWhiteSpace(rule.Id) Then
                errors.Add("Rule Id cannot be empty.")
            End If

            If String.IsNullOrWhiteSpace(rule.Name) Then
                errors.Add("Rule Name cannot be empty.")
            End If

            If rule.Priority < 0 Then
                errors.Add("Rule Priority must be non-negative.")
            End If

            If rule.Conditions IsNot Nothing AndAlso rule.Conditions.Count > 0 Then
                For i = 0 To rule.Conditions.Count - 1
                    Dim cond = rule.Conditions(i)
                    If String.IsNullOrWhiteSpace(cond.Field) Then
                        errors.Add($"Condition #{i + 1}: Field is required.")
                    End If
                    If String.IsNullOrWhiteSpace(cond.Operator) Then
                        errors.Add($"Condition #{i + 1}: Operator is required.")
                    End If
                    If cond.Value Is Nothing Then
                        errors.Add($"Condition #{i + 1}: Value is required.")
                    End If
                Next
            End If

            If rule.Actions IsNot Nothing AndAlso rule.Actions.Count > 0 Then
                For i = 0 To rule.Actions.Count - 1
                    Dim action = rule.Actions(i)
                    If String.IsNullOrWhiteSpace(action.Type) Then
                        errors.Add($"Action #{i + 1}: Type is required.")
                    End If
                Next
            End If

            Return errors
        End Function

        ''' <summary>
        ''' Creates a deep copy of a rule.
        ''' </summary>
        Public Function CloneRule(rule As Rule) As Rule
            Dim conditions As List(Of RuleCondition) = Nothing
            If rule.Conditions IsNot Nothing Then
                conditions = rule.Conditions.Select(Function(c) New RuleCondition With {
                    .Field = c.Field,
                    .Operator = c.Operator,
                    .Value = c.Value
                }).ToList()
            End If

            Dim actions As List(Of RuleAction) = Nothing
            If rule.Actions IsNot Nothing Then
                actions = rule.Actions.Select(Function(a) New RuleAction With {
                    .RuleId = a.RuleId,
                    .Type = a.Type,
                    .Value = a.Value,
                    .Parameters = If(a.Parameters IsNot Nothing, New Dictionary(Of String, String)(a.Parameters), Nothing)
                }).ToList()
            End If

            Return New Rule With {
                .Id = rule.Id,
                .Name = rule.Name,
                .Description = rule.Description,
                .IsEnabled = rule.IsEnabled,
                .Conditions = conditions,
                .Actions = actions,
                .Priority = rule.Priority,
                .RequiresTrustedSender = rule.RequiresTrustedSender,
                .AutomationLevel = rule.AutomationLevel,
                .ConditionGroupOperator = rule.ConditionGroupOperator,
                .CreatedAt = rule.CreatedAt,
                .ModifiedAt = rule.ModifiedAt
            }
        End Function

        ''' <summary>
        ''' Gets a human-readable description of an action type.
        ''' </summary>
        Public Function GetActionDisplayName(actionType As String) As String
            Select Case actionType.ToLowerInvariant()
                Case "createdraft"
                    Return "Create Draft Reply"
                Case "applytemplate"
                    Return "Apply Template"
                Case "forward"
                    Return "Forward Email"
                Case "flag"
                    Return "Flag for Follow-Up"
                Case "movetofolder"
                    Return "Move to Folder"
                Case "markread"
                    Return "Mark as Read"
                Case "categorize"
                    Return "Assign Category"
                Case "delete"
                    Return "Delete Email"
                Case "reply"
                    Return "Reply"
                Case "replyall"
                    Return "Reply All"
                Case Else
                    Return actionType
            End Select
        End Function

        ''' <summary>
        ''' Gets a human-readable description of a condition field.
        ''' </summary>
        Public Function GetFieldDisplayName(field As String) As String
            Select Case field.ToLowerInvariant()
                Case "classification"
                    Return "Classification"
                Case "sender"
                    Return "Sender (Email)"
                Case "senderdomain"
                    Return "Sender Domain"
                Case "subject"
                    Return "Subject"
                Case "body"
                    Return "Body"
                Case "torecipients"
                    Return "To Recipients"
                Case "ccrecipients"
                    Return "CC Recipients"
                Case "hasattachments"
                    Return "Has Attachments"
                Case "categories"
                    Return "Categories"
                Case "priority"
                    Return "Priority"
                Case "sentiment"
                    Return "Sentiment"
                Case Else
                    Return field
            End Select
        End Function

        ''' <summary>
        ''' Serializes a rule to JSON format for storage.
        ''' </summary>
        Public Function SerializeRule(rule As Rule) As String
            Return Newtonsoft.Json.JsonConvert.SerializeObject(rule,
                New Newtonsoft.Json.JsonSerializerSettings With {
                    .Formatting = Newtonsoft.Json.Formatting.Indented,
                    .NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
                })
        End Function

        ''' <summary>
        ''' Deserializes a rule from JSON format.
        ''' Returns Nothing if deserialization fails.
        ''' </summary>
        Public Function DeserializeRule(json As String) As Rule
            Try
                Return Newtonsoft.Json.JsonConvert.DeserializeObject(Of Rule)(json)
            Catch ex As Exception
                Return Nothing
            End Try
        End Function

    End Module

End Namespace