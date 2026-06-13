Imports System.Threading.Tasks
Imports OutlookAIAssistant.Core.Interfaces
Imports OutlookAIAssistant.Core.Models
Imports OutlookAIAssistant.Core.Enums
Imports OutlookAIAssistant.AIEngine.Scoring

Namespace OutlookAIAssistant.Rules

    ''' <summary>
    ''' Automation pipeline that orchestrates AI classification, rules evaluation,
    ''' trust scoring, and action execution across four automation levels:
    ''' 
    ''' Manual:        User clicks button -> AI generates -> User reviews -> User sends
    ''' AutomaticDraft: Email received -> AI generates draft -> Draft appears in Drafts folder
    ''' Supervised:    Email received -> AI generates -> Requires user approval -> Sends
    ''' Automatic:     Email received -> AI generates -> Sends directly (TrustScore >= 95%)
    ''' 
    ''' Trust scoring thresholds:
    '''   >= 95% : Can send automatically (Automatic level)
    '''   >= 65% : Can draft automatically (AutomaticDraft level)
    '''   < 65%  : Present for manual review (Supervised/Suggestions level)
    ''' </summary>
    Public Class AutomationPipeline

        Private ReadOnly _aiEngine As AIEngine.AIEngine
        Private ReadOnly _rulesEngine As RulesEngine
        Private ReadOnly _trustScorer As TrustScorer
        Private ReadOnly _templateEngine As TemplateEngine

        ''' <summary>Trust threshold for fully automatic send.</summary>
        Public Const ThresholdAutoSend As Double = 0.95

        ''' <summary>Trust threshold for automatic draft creation.</summary>
        Public Const ThresholdAutoDraft As Double = 0.65

        Public Sub New(aiEngine As AIEngine.AIEngine, rulesEngine As RulesEngine)
            _aiEngine = aiEngine
            _rulesEngine = rulesEngine
            _trustScorer = New TrustScorer()
            _templateEngine = New TemplateEngine()
        End Sub

        ''' <summary>
        ''' Processes an email through the full automation pipeline.
        ''' 
        ''' Flow:
        ''' 1. AI Classification - categorize the email content
        ''' 2. Rules Evaluation - match conditions and collect actions
        ''' 3. Trust Scoring - determine sender trust level
        ''' 4. Action Execution - apply actions per the configured automation level
        ''' </summary>
        Public Async Function ProcessAsync(
            email As EmailMessage,
            Optional automationLevel As AutomationLevel = AutomationLevel.Suggestions,
            Optional senderTrustScore As TrustScore = Nothing) As Task(Of PipelineResult)

            Dim result = New PipelineResult()
            result.AppliedAutomationLevel = automationLevel

            Try
                ' Step 1: AI Classification
                Dim classification = Await ClassifyEmailAsync(email)
                If classification IsNot Nothing Then
                    email.Classification = classification
                    email.IsAICategorized = True
                End If

                ' Step 2: Evaluate rules against the classified email
                Dim actions = Await _rulesEngine.EvaluateAsync(email)
                result.Actions = New List(Of RuleAction)(actions)
                result.Processed = actions.Any()

                ' Step 3: Trust Scoring
                Dim trustScore = Await ComputeTrustScoreAsync(email, senderTrustScore)
                result.SenderTrustScore = trustScore

                ' Step 4: Process actions based on automation level and trust score
                If actions.Any() Then
                    Await ProcessActionsByLevelAsync(actions, email, automationLevel, trustScore, result)
                End If

                Dim summary = BuildResultSummary(classification, actions.Count, automationLevel, trustScore)
                result.Summary = summary

            Catch ex As Exception
                result.Errors.Add(ex.Message)
                result.Summary = "Pipeline error: " & ex.Message
            End Try

            Return result
        End Function

        ''' <summary>
        ''' Classifies an email using the AI engine if not already classified.
        ''' </summary>
        Private Async Function ClassifyEmailAsync(email As EmailMessage) As Task(Of Classification)
            If email.Classification IsNot Nothing AndAlso email.IsAICategorized Then
                Return email.Classification
            End If

            Dim configuredProviders = _aiEngine.GetConfiguredProviders()
            If configuredProviders Is Nothing OrElse configuredProviders.Count = 0 Then
                Return Nothing
            End If

            Try
                Dim request As New AIRequest()
                request.Content = email.Body
                request.Subject = email.Subject
                request.ProviderName = configuredProviders.First()
                Return Await _aiEngine.ClassifyAsync(request)
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine("Classification error: " & ex.Message)
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Computes the trust score for the email sender.
        ''' Uses a baseline score from classification confidence, with domain-level boost.
        ''' </summary>
        Private Async Function ComputeTrustScoreAsync(
            email As EmailMessage,
            Optional existingScore As TrustScore = Nothing) As Task(Of TrustScore)

            If existingScore IsNot Nothing Then
                Return existingScore
            End If

            ' Compute a basic trust score from available data
            ' In production, this would look up sender history from SQLite
            Dim score As Double

            ' If the email was categorized with high confidence, boost trust
            If email.Classification IsNot Nothing Then
                score = 0.3 + (email.Classification.Confidence * 0.5)
            Else
                score = 0.3 ' Neutral baseline
            End If

            ' If sender domain looks like a business domain, boost slightly
            If Not String.IsNullOrEmpty(email.SenderEmail) AndAlso
               (Not email.SenderEmail.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase)) AndAlso
               (Not email.SenderEmail.EndsWith("@yahoo.com", StringComparison.OrdinalIgnoreCase)) AndAlso
               (Not email.SenderEmail.EndsWith("@hotmail.com", StringComparison.OrdinalIgnoreCase)) Then
                score += 0.1
            End If

            ' Clamp between 0.0 and 1.0
            score = Math.Min(1.0, Math.Max(0.0, score))

            Dim ts As New TrustScore()
            ts.Identifier = email.SenderEmail
            ts.Score = score
            ts.InteractionCount = 1
            ts.ResponseRate = 0.5
            Return ts
        End Function

        ''' <summary>
        ''' Processes matched actions according to the configured automation level
        ''' and the computed trust score.
        ''' </summary>
        Private Async Function ProcessActionsByLevelAsync(
            actions As IReadOnlyList(Of RuleAction),
            email As EmailMessage,
            level As AutomationLevel,
            trustScore As TrustScore,
            result As PipelineResult) As Task

            Dim trustValue As Double
            Dim isWhitelisted As Boolean = False

            If trustScore IsNot Nothing Then
                trustValue = trustScore.Score
                isWhitelisted = trustScore.IsWhitelisted
            Else
                trustValue = 0.0
            End If

            For Each action In actions
                Select Case level
                    Case AutomationLevel.Manual
                        ' Manual: no auto-execution, just log suggestions
                        System.Diagnostics.Debug.WriteLine(
                            "[Manual] Suggested action: " & action.Type & " on '" & email.Subject & "'")

                    Case AutomationLevel.Suggestions
                        ' Suggestions: AI suggests but everything requires user approval
                        System.Diagnostics.Debug.WriteLine(
                            "[Suggestions] Suggested action: " & action.Type & " on '" & email.Subject & "'")

                    Case AutomationLevel.AutomaticDraft
                        ' AutomaticDraft: create drafts automatically if trust is sufficient
                        If isWhitelisted OrElse trustValue >= ThresholdAutoDraft Then
                            If action.Type = "createdraft" OrElse
                               action.Type = "applytemplate" OrElse
                               action.Type = "reply" Then
                                Dim reply = Await GenerateReplyContentAsync(action, email)
                                If Not String.IsNullOrEmpty(reply) Then
                                    result.GeneratedReply = reply
                                    result.ExecutedActions.Add(action)
                                    System.Diagnostics.Debug.WriteLine(
                                        "[AutoDraft] Created draft for '" & email.Subject & "'")
                                End If
                            End If
                        Else
                            System.Diagnostics.Debug.WriteLine(
                                "[AutoDraft] Trust too low (" & trustValue.ToString("P0") & _
                                ") for '" & email.Subject & "'. Falling to suggestion.")
                        End If

                    Case AutomationLevel.Supervised
                        ' Supervised: AI generates reply but requires user approval
                        If action.Type = "createdraft" OrElse
                           action.Type = "applytemplate" OrElse
                           action.Type = "reply" Then
                            Dim reply = Await GenerateReplyContentAsync(action, email)
                            If Not String.IsNullOrEmpty(reply) Then
                                result.GeneratedReply = reply
                                result.ExecutedActions.Add(action)
                                System.Diagnostics.Debug.WriteLine(
                                    "[Supervised] Generated reply for '" & email.Subject & "' (pending approval)")
                            End If
                        Else
                            ' Non-reply actions execute immediately (move, flag, categorize)
                            ExecuteAction(action, email)
                            result.ExecutedActions.Add(action)
                        End If

                    Case AutomationLevel.Automatic
                        ' Automatic: can send directly if trust score is sufficient
                        If isWhitelisted OrElse trustValue >= ThresholdAutoSend Then
                            If action.Type = "createdraft" OrElse
                               action.Type = "applytemplate" OrElse
                               action.Type = "reply" OrElse
                               action.Type = "replyall" Then
                                Dim reply = Await GenerateReplyContentAsync(action, email)
                                If Not String.IsNullOrEmpty(reply) Then
                                    result.GeneratedReply = reply
                                    result.WasAutoSent = True
                                    result.ExecutedActions.Add(action)
                                    System.Diagnostics.Debug.WriteLine(
                                        "[Automatic] Auto-sent reply for '" & email.Subject & _
                                        "' (trust: " & trustValue.ToString("P0") & ")")
                                End If
                            Else
                                ' Non-reply actions execute immediately
                                ExecuteAction(action, email)
                                result.ExecutedActions.Add(action)
                            End If
                        ElseIf trustValue >= ThresholdAutoDraft Then
                            ' Trust >= 65% but < 95%: create draft instead
                            Dim reply = Await GenerateReplyContentAsync(action, email)
                            If Not String.IsNullOrEmpty(reply) Then
                                result.GeneratedReply = reply
                                result.ExecutedActions.Add(action)
                                System.Diagnostics.Debug.WriteLine(
                                    "[Automatic] Trust " & trustValue.ToString("P0") & _
                                    " below auto-send. Created draft for '" & email.Subject & "'.")
                            End If
                        Else
                            ' Trust < 65%: generate but present for supervised review
                            Dim reply = Await GenerateReplyContentAsync(action, email)
                            If Not String.IsNullOrEmpty(reply) Then
                                result.GeneratedReply = reply
                                System.Diagnostics.Debug.WriteLine(
                                    "[Automatic] Trust too low (" & trustValue.ToString("P0") & _
                                    "). Presenting '" & email.Subject & "' for review.")
                            End If
                        End If

                    Case Else
                        System.Diagnostics.Debug.WriteLine(
                            "[Unknown] Level " & level.ToString() & _
                            ": no action taken for '" & email.Subject & "'")
                End Select
            Next
        End Function

        ''' <summary>
        ''' Generates reply content for a given action, either from a template
        ''' or via the AI engine.
        ''' </summary>
        Private Async Function GenerateReplyContentAsync(
            action As RuleAction,
            email As EmailMessage) As Task(Of String)

            If action.Type = "applytemplate" AndAlso Not String.IsNullOrEmpty(action.Value) Then
                ' Use named template
                Try
                    Dim parameters = _templateEngine.BuildEmailParameters(email)
                    Return _templateEngine.RenderNamed(action.Value, parameters)
                Catch ex As KeyNotFoundException
                    System.Diagnostics.Debug.WriteLine("Template '" & action.Value & "' not found.")
                End Try
            End If

            ' For createdraft/reply actions, use AI to generate content
            If action.Type = "createdraft" OrElse action.Type = "reply" OrElse
               action.Type = "replyall" Then

                Dim configuredProviders = _aiEngine.GetConfiguredProviders()
                If configuredProviders IsNot Nothing AndAlso configuredProviders.Count > 0 Then
                    Try
                        Dim request As New AIRequest()
                        request.Content = email.Body
                        request.Subject = email.Subject
                        request.SenderEmail = email.SenderEmail
                        request.ProviderName = configuredProviders.First()

                        ' If parameters specify instructions, pass them
                        If action.Parameters IsNot Nothing AndAlso
                           action.Parameters.ContainsKey("instructions") Then
                            request.Instructions = action.Parameters("instructions")
                        End If

                        Dim response = Await _aiEngine.GenerateReplyAsync(request)
                        If response IsNot Nothing AndAlso response.IsSuccess Then
                            Return response.Content
                        End If
                    Catch ex As Exception
                        System.Diagnostics.Debug.WriteLine("AI reply generation error: " & ex.Message)
                    End Try
                End If
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' Executes a non-reply action (move, flag, mark read, categorize, etc.).
        ''' In production, this would use the Outlook object model.
        ''' </summary>
        Private Sub ExecuteAction(action As RuleAction, email As EmailMessage)
            Select Case action.Type.ToLowerInvariant()
                Case "movetofolder"
                    System.Diagnostics.Debug.WriteLine(
                        "Move '" & email.Subject & "' to folder: " & action.Value)

                Case "flag"
                    System.Diagnostics.Debug.WriteLine(
                        "Flag '" & email.Subject & "' for follow-up")

                Case "markread"
                    System.Diagnostics.Debug.WriteLine(
                        "Mark '" & email.Subject & "' as read")

                Case "categorize"
                    System.Diagnostics.Debug.WriteLine(
                        "Categorize '" & email.Subject & "' as: " & action.Value)

                Case "delete"
                    System.Diagnostics.Debug.WriteLine(
                        "Delete '" & email.Subject & "'")

                Case "forward"
                    System.Diagnostics.Debug.WriteLine(
                        "Forward '" & email.Subject & "' to: " & action.Value)

                Case Else
                    System.Diagnostics.Debug.WriteLine(
                        "Unknown action '" & action.Type & "' for '" & email.Subject & "'")
            End Select
        End Sub

        ''' <summary>
        ''' Builds a human-readable summary of the pipeline execution.
        ''' </summary>
        Private Function BuildResultSummary(
            classification As Classification,
            actionCount As Integer,
            level As AutomationLevel,
            trustScore As TrustScore) As String

            Dim parts As New List(Of String)()

            If classification IsNot Nothing Then
                parts.Add("Classified as '" & classification.Type.ToString() & _
                          "' (confidence: " & classification.Confidence.ToString("P0") & ")")
            Else
                parts.Add("Not classified")
            End If

            parts.Add(actionCount.ToString() & " rule(s) triggered")

            Dim levelName = [Enum].GetName(GetType(AutomationLevel), level)
            parts.Add("Automation level: " & levelName)

            If trustScore IsNot Nothing Then
                parts.Add("Trust score: " & trustScore.Score.ToString("P0"))
            End If

            Return String.Join(" | ", parts)
        End Function

        ''' <summary>
        ''' Shorthand: processes at the AutomaticDraft level for incoming email.
        ''' </summary>
        Public Async Function ProcessWithAutoDraftAsync(email As EmailMessage) As Task(Of PipelineResult)
            Return Await ProcessAsync(email, AutomationLevel.AutomaticDraft)
        End Function

        ''' <summary>
        ''' Shorthand: processes at the Automatic level with full auto-send.
        ''' </summary>
        Public Async Function ProcessWithFullAutoAsync(email As EmailMessage) As Task(Of PipelineResult)
            Return Await ProcessAsync(email, AutomationLevel.Automatic)
        End Function

    End Class

End Namespace