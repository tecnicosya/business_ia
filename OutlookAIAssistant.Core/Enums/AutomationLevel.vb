Namespace OutlookAIAssistant.Core.Enums

    ''' <summary>
    ''' Defines the level of automation for email processing.
    ''' Maps to the four automation modes in the business plan:
    ''' 
    ''' Manual:         User clicks button -> AI generates -> User reviews -> User sends
    ''' AutomaticDraft: Email received -> AI generates draft -> Draft appears in Drafts folder
    ''' Supervised:     Email received -> AI generates -> Requires user approval -> Sends
    ''' Automatic:      Email received -> AI generates -> Sends directly (TrustScore >= 95%)
    ''' </summary>
    Public Enum AutomationLevel
        ''' <summary>Manual — no automation, all actions user-initiated.</summary>
        Manual = 0

        ''' <summary>AutomaticDraft — AI generates a draft; user reviews and sends.</summary>
        AutomaticDraft = 1

        ''' <summary>Supervised — AI generates reply; requires user approval before sending.</summary>
        Supervised = 2

        ''' <summary>Automatic — fully automated; sends directly when trust >= 95%.</summary>
        Automatic = 3

        ''' <summary>Legacy alias: Suggestions — AI suggests actions requiring user approval.</summary>
        Suggestions = 1

        ''' <summary>Legacy alias: Partial — auto-reply for trusted senders, suggestions otherwise.</summary>
        Partial = 2

        ''' <summary>Legacy alias: Full — fully automated processing for trusted senders.</summary>
        Full = 3
    End Enum

End Namespace