Namespace OutlookAIAssistant.Core.Enums

    ''' <summary>
    ''' Defines the level of automation for email processing.
    ''' Ranges from fully manual to fully automated.
    ''' </summary>
    Public Enum AutomationLevel
        ''' <summary>Manual — no automation, all actions user-initiated.</summary>
        Manual = 0

        ''' <summary>Suggestions — AI suggests actions but requires user approval.</summary>
        Suggestions = 1

        ''' <summary>Partial — auto-reply only for trusted senders, suggestions for others.</summary>
        Partial = 2

        ''' <summary>Full — fully automated processing for all trusted senders.</summary>
        Full = 3
    End Enum

End Namespace