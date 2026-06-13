Imports System.Text.RegularExpressions
Imports System.Threading.Tasks
Imports OutlookAIAssistant.Core.Interfaces
Imports OutlookAIAssistant.Core.Models
Imports OutlookAIAssistant.Core.Enums

Namespace OutlookAIAssistant.Rules

    ''' <summary>
    ''' Rules engine that evaluates typed conditions against email messages
    ''' and returns matched actions. Supports compound AND/OR condition groups,
    ''' priority-based ordering, and trusted-sender filtering.
    ''' </summary>
    Public Class RulesEngine
        Implements IRulesEngine

        Private ReadOnly _rules As New List(Of Rule)()
        Private ReadOnly _lock As New Object()

        ''' <summary>
        ''' Evaluates all enabled rules against the given email and returns
        ''' sorted, aggregated actions from all matching rules.
        ''' </summary>
        Public Async Function EvaluateAsync(email As EmailMessage) As Task(Of IReadOnlyList(Of RuleAction)) Implements IRulesEngine.EvaluateAsync
            Dim actions As New List(Of RuleAction)()

            Await Task.Run(Sub()
                SyncLock _lock
                    Dim enabledRules = _rules.Where(Function(r) r.IsEnabled).
                                              OrderBy(Function(r) r.Priority).
                                              ToList()

                    For Each rule In enabledRules
                        If EvaluateConditions(rule, email) Then
                            For Each action In rule.Actions
                                actions.Add(New RuleAction With {
                                    .RuleId = rule.Id,
                                    .Type = action.Type,
                                    .Value = action.Value,
                                    .Parameters = If(action.Parameters IsNot Nothing,
                                                     New Dictionary(Of String, String)(action.Parameters),
                                                     Nothing)
                                })
                            Next
                        End If
                    Next
                End SyncLock
            End Sub)

            Return actions.AsReadOnly()
        End Function

        ''' <summary>
        ''' Adds (or replaces) a rule in the engine. Validates the rule before adding.
        ''' Throws ArgumentException if validation fails.
        ''' </summary>
        Public Sub AddRule(rule As Rule) Implements IRulesEngine.AddRule
            Dim errors = RuleHelper.ValidateRule(rule)
            If errors.Count > 0 Then
                Throw New ArgumentException($"Rule validation failed: {String.Join("; ", errors)}")
            End If

            SyncLock _lock
                Dim existing = _rules.FirstOrDefault(Function(r) r.Id = rule.Id)
                If existing IsNot Nothing Then
                    _rules.Remove(existing)
                End If
                rule.ModifiedAt = DateTime.UtcNow
                _rules.Add(rule)
            End SyncLock
        End Sub

        ''' <summary>
        ''' Removes a rule by its ID.
        ''' </summary>
        Public Sub RemoveRule(ruleId As String) Implements IRulesEngine.RemoveRule
            SyncLock _lock
                _rules.RemoveAll(Function(r) r.Id = ruleId)
            End SyncLock
        End Sub

        ''' <summary>
        ''' Returns a snapshot of all registered rules, ordered by priority.
        ''' </summary>
        Public Function GetRules() As IReadOnlyList(Of Rule) Implements IRulesEngine.GetRules
            SyncLock _lock
                Return _rules.OrderBy(Function(r) r.Priority).ToList().AsReadOnly()
            End SyncLock
        End Function

        ''' <summary>
        ''' Returns the count of currently registered rules.
        ''' </summary>
        Public Function GetRuleCount() As Integer
            SyncLock _lock
                Return _rules.Count
            End SyncLock
        End Function

        ''' <summary>
        ''' Clears all rules from the engine.
        ''' </summary>
        Public Sub ClearRules()
            SyncLock _lock
                _rules.Clear()
            End SyncLock
        End Sub

        ''' <summary>
        ''' Runs the full pipeline: evaluate rules against an email and produce a PipelineResult.
        ''' </summary>
        Public Async Function RunPipelineAsync(email As EmailMessage) As Task(Of PipelineResult) Implements IRulesEngine.RunPipelineAsync
            Dim result = New PipelineResult()

            Try
                Dim actions = Await EvaluateAsync(email)
                result.Actions = New List(Of RuleAction)(actions)
                result.Processed = actions.Any()
                result.Summary = $"Evaluated {GetRuleCount()} rules, triggered {actions.Count} action(s)."
            Catch ex As Exception
                result.Errors.Add(ex.Message)
                result.Summary = $"Pipeline error: {ex.Message}"
            End Try

            Return result
        End Function

        ''' <summary>
        ''' Evaluates all conditions within a rule against an email.
        ''' Supports compound AND/OR logic via ConditionGroupOperator.
        ''' </summary>
        Private Function EvaluateConditions(rule As Rule, email As EmailMessage) As Boolean
            If rule.Conditions Is Nothing OrElse rule.Conditions.Count = 0 Then
                ' No conditions means the rule matches everything
                Return True
            End If

            If rule.RequiresTrustedSender Then
                ' If the rule requires a trusted sender but we can't verify,
                ' skip it. (The caller should set IsTrustedSender on the email or
                ' pass trust info separately.)
                If Not IsSenderTrusted(email) Then
                    Return False
                End If
            End If

            ' Evaluate all conditions
            Dim results As New List(Of Boolean)()
            For Each condition In rule.Conditions
                results.Add(EvaluateSingleCondition(condition, email))
            Next

            ' Combine based on group operator
            If rule.ConditionGroupOperator = ConditionGroupOperator.And Then
                Return results.All(Function(r) r)
            Else
                Return results.Any(Function(r) r)
            End If
        End Function

        ''' <summary>
        ''' Evaluates a single condition against an email.
        ''' </summary>
        Private Function EvaluateSingleCondition(condition As RuleCondition, email As EmailMessage) As Boolean
            Dim fieldValue = GetFieldValue(condition.Field, email)
            Dim operatorValue = If(condition.Operator, "").ToLowerInvariant()
            Dim compareValue = If(condition.Value, "")

            Select Case operatorValue
                Case "equals"
                    Return String.Equals(fieldValue, compareValue, StringComparison.OrdinalIgnoreCase)
                Case "notequals"
                    Return Not String.Equals(fieldValue, compareValue, StringComparison.OrdinalIgnoreCase)
                Case "contains"
                    Return fieldValue.IndexOf(compareValue, StringComparison.OrdinalIgnoreCase) >= 0
                Case "notcontains"
                    Return fieldValue.IndexOf(compareValue, StringComparison.OrdinalIgnoreCase) < 0
                Case "startswith"
                    Return fieldValue.StartsWith(compareValue, StringComparison.OrdinalIgnoreCase)
                Case "endswith"
                    Return fieldValue.EndsWith(compareValue, StringComparison.OrdinalIgnoreCase)
                Case "in"
                    Return compareValue.Split(";"c).
                        Any(Function(s) String.Equals(s.Trim(), fieldValue, StringComparison.OrdinalIgnoreCase))
                Case "notin"
                    Return Not compareValue.Split(";"c).
                        Any(Function(s) String.Equals(s.Trim(), fieldValue, StringComparison.OrdinalIgnoreCase))
                Case "matchesregex"
                    Return Regex.IsMatch(fieldValue, compareValue, RegexOptions.IgnoreCase)
                Case "greaterthan"
                    Return CompareNumericValues(fieldValue, compareValue) > 0
                Case "lessthan"
                    Return CompareNumericValues(fieldValue, compareValue) < 0
                Case "istrue"
                    Return String.Equals(fieldValue, "true", StringComparison.OrdinalIgnoreCase)
                Case "isfalse"
                    Return Not String.Equals(fieldValue, "true", StringComparison.OrdinalIgnoreCase)
                Case Else
                    ' Default: fall back to contains for backwards compatibility
                    Return fieldValue.IndexOf(compareValue, StringComparison.OrdinalIgnoreCase) >= 0
            End Select
        End Function

        ''' <summary>
        ''' Extracts the relevant field value from an email message for condition evaluation.
        ''' </summary>
        Private Function GetFieldValue(field As String, email As EmailMessage) As String
            Select Case field.ToLowerInvariant()
                Case "classification"
                    Return email.Classification?.Type?.ToString() ?? ""
                Case "subcategory"
                    Return email.Classification?.SubCategory ?? ""
                Case "sender"
                    Return email.SenderEmail ?? ""
                Case "senderdomain"
                    Dim emailAddr = email.SenderEmail ?? ""
                    Dim atIndex = emailAddr.LastIndexOf("@"c)
                    Return If(atIndex >= 0, emailAddr.Substring(atIndex + 1), "")
                Case "sendername"
                    Return email.SenderName ?? ""
                Case "subject"
                    Return email.Subject ?? ""
                Case "body"
                    Return email.Body ?? ""
                Case "torecipients"
                    Return email.ToRecipients ?? ""
                Case "ccrecipients"
                    Return email.CcRecipients ?? ""
                Case "hasattachments"
                    Return If(email.HasAttachments, "true", "false")
                Case "categories"
                    Return If(email.Categories IsNot Nothing, String.Join(";", email.Categories), "")
                Case "priority"
                    Return email.Classification?.Priority ?? ""
                Case "sentiment"
                    Return email.Classification?.Sentiment ?? ""
                Case "isread"
                    Return If(email.IsRead, "true", "false")
                Case "conversationid"
                    Return email.ConversationId ?? ""
                Case Else
                    Return ""
            End Select
        End Function

        ''' <summary>
        ''' Determines if the sender of this email is trusted.
        ''' Checks classification metadata or other trust indicators.
        ''' </summary>
        Private Function IsSenderTrusted(email As EmailMessage) As Boolean
            ' If AI classification marked it with sufficient confidence, consider trusted
            If email.Classification IsNot Nothing AndAlso email.Classification.Confidence >= 0.7 Then
                Return True
            End If

            ' If the email was AI-categorized, it's been vetted
            If email.IsAICategorized Then
                Return True
            End If

            Return False
        End Function

        ''' <summary>
        ''' Compares two string values as numbers. Returns -1, 0, or 1.
        ''' </summary>
        Private Function CompareNumericValues(left As String, right As String) As Integer
            Dim lVal As Double
            Dim rVal As Double

            If Double.TryParse(left, lVal) AndAlso Double.TryParse(right, rVal) Then
                Return lVal.CompareTo(rVal)
            End If

            ' Fall back to string comparison
            Return String.Compare(left, right, StringComparison.OrdinalIgnoreCase)
        End Function

    End Class

End Namespace