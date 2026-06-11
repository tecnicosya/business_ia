Imports OutlookAIAssistant.Core.Interfaces
Imports OutlookAIAssistant.Core.Models

Namespace OutlookAIAssistant.Rules

    ''' <summary>
    ''' Automation pipeline that runs emails through AI classification, rules evaluation, and actions.
    ''' </summary>
    Public Class AutomationPipeline

        Private ReadOnly _aiEngine As AIEngine.AIEngine
        Private ReadOnly _rulesEngine As RulesEngine

        Public Sub New(aiEngine As AIEngine.AIEngine, rulesEngine As RulesEngine)
            _aiEngine = aiEngine
            _rulesEngine = rulesEngine
        End Sub

        ''' <summary>
        ''' Processes an email through the full pipeline:
        ''' 1. AI Classification
        ''' 2. Rules Evaluation
        ''' 3. Action Execution
        ''' </summary>
        Public Async Function ProcessAsync(email As EmailMessage,
                                           Optional automationLevel As Core.Enums.AutomationLevel = Core.Enums.AutomationLevel.Suggestions) As Task(Of PipelineResult)

            Dim result = New PipelineResult()

            Try
                ' Step 1: AI Classification
                Dim classification = Await _aiEngine.ClassifyAsync(
                    New AIRequest With {
                        .Content = email.Body,
                        .Subject = email.Subject,
                        .ProviderName = _aiEngine.GetConfiguredProviders().FirstOrDefault()
                    }
                )

                If classification IsNot Nothing Then
                    email.Classification = classification
                    email.IsAICategorized = True
                End If

                ' Step 2: Evaluate rules
                Dim actions = Await _rulesEngine.EvaluateAsync(email)
                result.Actions = New List(Of RuleAction)(actions)
                result.Processed = actions.Any()

                ' Step 3: Execute actions based on automation level
                For Each action In actions
                    Select Case automationLevel
                        Case Core.Enums.AutomationLevel.Full
                            ExecuteAction(action, email)
                        Case Core.Enums.AutomationLevel.Partial
                            ' Only auto-execute for trusted senders
                            If email.Classification IsNot Nothing AndAlso
                               email.Classification.Confidence >= 0.8 Then
                                ExecuteAction(action, email)
                            End If
                        Case Else
                            ' Suggestions — just log, don't auto-execute
                            System.Diagnostics.Debug.WriteLine($"Suggested action: {action.Type} on '{email.Subject}'")
                    End Select
                Next

                result.Summary = $"Pipeline complete. Classified as '{classification?.Type}', " &
                                 $"{actions.Count} rule(s) triggered."

            Catch ex As Exception
                result.Errors = New List(Of String) From {ex.Message}
                result.Summary = $"Pipeline error: {ex.Message}"
            End Try

            Return result
        End Function

        Private Sub ExecuteAction(action As RuleAction, email As EmailMessage)
            System.Diagnostics.Debug.WriteLine($"Executing action '{action.Type}' on '{email.Subject}'")
            ' Production: move to folder, flag, mark read, etc.
        End Sub

    End Class

End Namespace