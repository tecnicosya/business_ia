Imports System.Threading.Tasks
Imports OutlookAIAssistant.Core.Interfaces
Imports OutlookAIAssistant.Core.Models

Namespace OutlookAIAssistant.Rules

    ''' <summary>
    ''' Rules engine that evaluates conditions and executes actions on emails.
    ''' </summary>
    Public Class RulesEngine
        Implements IRulesEngine

        Private ReadOnly _rules As New List(Of Rule)()
        Private ReadOnly _lock As New Object()

        Public Async Function EvaluateAsync(email As EmailMessage) As Task(Of IReadOnlyList(Of RuleAction)) Implements IRulesEngine.EvaluateAsync
            Dim actions As New List(Of RuleAction)()

            Await Task.Run(Sub()
                SyncLock _lock
                    ' Sort by priority (lower = higher priority)
                    Dim enabledRules = _rules.Where(Function(r) r.IsEnabled).OrderBy(Function(r) r.Priority).ToList()

                    For Each rule In enabledRules
                        If EvaluateConditions(rule, email) Then
                            Dim ruleActions = ParseActions(rule)
                            actions.AddRange(ruleActions)
                        End If
                    Next
                End SyncLock
            End Sub)

            Return actions.AsReadOnly()
        End Function

        Public Sub AddRule(rule As Rule) Implements IRulesEngine.AddRule
            SyncLock _lock
                ' Ensure unique ID
                Dim existing = _rules.FirstOrDefault(Function(r) r.Id = rule.Id)
                If existing IsNot Nothing Then
                    _rules.Remove(existing)
                End If
                _rules.Add(rule)
            End SyncLock
        End Sub

        Public Sub RemoveRule(ruleId As String) Implements IRulesEngine.RemoveRule
            SyncLock _lock
                _rules.RemoveAll(Function(r) r.Id = ruleId)
            End SyncLock
        End Sub

        Public Function GetRules() As IReadOnlyList(Of Rule) Implements IRulesEngine.GetRules
            SyncLock _lock
                Return _rules.ToList().AsReadOnly()
            End SyncLock
        End Function

        Public Async Function RunPipelineAsync(email As EmailMessage) As Task(Of PipelineResult) Implements IRulesEngine.RunPipelineAsync
            Dim result = New PipelineResult()

            Try
                Dim actions = Await EvaluateAsync(email)
                result.Actions = New List(Of RuleAction)(actions)
                result.Processed = actions.Any()
                result.Summary = $"Evaluated {_rules.Count} rules, triggered {actions.Count} actions."
            Catch ex As Exception
                result.Errors = New List(Of String) From {ex.Message}
                result.Summary = $"Pipeline error: {ex.Message}"
            End Try

            Return result
        End Function

        Private Function EvaluateConditions(rule As Rule, email As EmailMessage) As Boolean
            ' Simplified condition evaluation
            ' Production implementation would parse JSON conditions and evaluate them
            If String.IsNullOrEmpty(rule.Conditions) Then Return True
            Return True ' Placeholder
        End Function

        Private Function ParseActions(rule As Rule) As List(Of RuleAction)
            ' Simplified action parsing
            ' Production implementation would parse JSON actions
            Dim actions As New List(Of RuleAction)()
            If Not String.IsNullOrEmpty(rule.Actions) Then
                ' Placeholder: parse from JSON
            End If
            Return actions
        End Function

    End Class

End Namespace