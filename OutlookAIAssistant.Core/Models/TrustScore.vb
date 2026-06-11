Namespace OutlookAIAssistant.Core.Models

    ''' <summary>
    ''' Represents a trust score for an email sender or domain.
    ''' Used by the AI to determine how confidently to auto-process.
    ''' </summary>
    Public Class TrustScore
        ''' <summary>The email address or domain this score applies to.</summary>
        Public Property Identifier As String

        ''' <summary>Whether this is a domain-level score (vs. email address).</summary>
        Public Property IsDomainLevel As Boolean = False

        ''' <summary>Numeric score 0.0 (untrusted) to 1.0 (fully trusted).</summary>
        Public Property Score As Double

        ''' <summary>Number of interactions used to calculate this score.</summary>
        Public Property InteractionCount As Integer

        ''' <summary>Average response rate from this sender.</summary>
        Public Property ResponseRate As Double

        ''' <summary>Whether the sender was manually whitelisted.</summary>
        Public Property IsWhitelisted As Boolean = False

        ''' <summary>Whether the sender was manually blacklisted.</summary>
        Public Property IsBlacklisted As Boolean = False

        ''' <summary>Timestamp of the last interaction.</summary>
        Public Property LastInteraction As DateTime?

        ''' <summary>Returns True if the score is sufficient for automated processing.</summary>
        Public Function IsTrusted(Optional threshold As Double = 0.7) As Boolean
            Return IsWhitelisted OrElse (Score >= threshold AndAlso Not IsBlacklisted)
        End Function
    End Class

End Namespace