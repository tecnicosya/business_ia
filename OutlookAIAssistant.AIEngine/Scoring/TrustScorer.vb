Imports OutlookAIAssistant.Core.Models

Namespace OutlookAIAssistant.AIEngine.Scoring

    ''' <summary>
    ''' Calculates trust scores for email senders based on interaction history.
    ''' Used to determine automation confidence.
    ''' </summary>
    Public Class TrustScorer

        ''' <summary>
        ''' Calculates a trust score for a sender based on interaction data.
        ''' </summary>
        Public Function CalculateScore(interactionCount As Integer, responseRate As Double, spamReports As Integer, Optional manualScore As Double? = Nothing) As TrustScore
            If manualScore.HasValue Then
                Return New TrustScore With {.Score = Math.Min(1.0, Math.Max(0.0, manualScore.Value))}
            End If

            ' Base score starts at 0.3 (neutral)
            Dim score = 0.3

            ' Increase based on number of positive interactions
            score += Math.Min(0.4, interactionCount * 0.05)

            ' Increase based on response rate
            score += responseRate * 0.2

            ' Decrease based on spam reports
            score -= Math.Min(0.5, spamReports * 0.15)

            ' Clamp between 0.0 and 1.0
            score = Math.Min(1.0, Math.Max(0.0, score))

            Return New TrustScore With {
                .Score = score,
                .InteractionCount = interactionCount,
                .ResponseRate = responseRate
            }
        End Function

        ''' <summary>
        ''' Determines if automated processing is safe for a given trust score.
        ''' </summary>
        Public Function IsAutomationSafe(score As Double, Optional threshold As Double = 0.7) As Boolean
            Return score >= threshold
        End Function

    End Class

End Namespace