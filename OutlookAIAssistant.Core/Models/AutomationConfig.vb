Imports OutlookAIAssistant.Core.Enums

Namespace OutlookAIAssistant.Core.Models

    ''' <summary>
    ''' Configuration for an automation level, wrapping the enum with additional metadata.
    ''' Maps to the four business plan automation modes.
    ''' </summary>
    Public Class AutomationConfig
        ''' <summary>The automation level.</summary>
        Public Property Level As AutomationLevel

        ''' <summary>Display name for the configuration.</summary>
        Public Property DisplayName As String

        ''' <summary>Description of what this level does.</summary>
        Public Property Description As String

        ''' <summary>Whether auto-reply is enabled at this level.</summary>
        Public Property EnableAutoReply As Boolean

        ''' <summary>Whether auto-categorization is enabled.</summary>
        Public Property EnableAutoCategorize As Boolean

        ''' <summary>Whether AI summarization runs automatically.</summary>
        Public Property EnableAutoSummarize As Boolean

        ''' <summary>Whether automated rules execute without manual approval.</summary>
        Public Property EnableAutoRules As Boolean

        ''' <summary>Creates a default configuration for a given automation level.</summary>
        Public Shared Function CreateDefault(level As AutomationLevel) As AutomationConfig
            Select Case level
                Case Enums.AutomationLevel.Manual
                    Return New AutomationConfig With {
                        .Level = level,
                        .DisplayName = "Manual",
                        .Description = "User clicks button -> AI generates -> User reviews -> User sends",
                        .EnableAutoReply = False,
                        .EnableAutoCategorize = False,
                        .EnableAutoSummarize = False,
                        .EnableAutoRules = False
                    }
                Case Enums.AutomationLevel.AutomaticDraft, Enums.AutomationLevel.Suggestions
                    Return New AutomationConfig With {
                        .Level = level,
                        .DisplayName = "Automatic Draft",
                        .Description = "Email received -> AI generates draft -> Draft appears in Drafts folder",
                        .EnableAutoReply = False,
                        .EnableAutoCategorize = True,
                        .EnableAutoSummarize = True,
                        .EnableAutoRules = True
                    }
                Case Enums.AutomationLevel.Supervised, Enums.AutomationLevel.Partial
                    Return New AutomationConfig With {
                        .Level = level,
                        .DisplayName = "Supervised",
                        .Description = "Email received -> AI generates -> Requires user approval -> Sends",
                        .EnableAutoReply = False,
                        .EnableAutoCategorize = True,
                        .EnableAutoSummarize = True,
                        .EnableAutoRules = True
                    }
                Case Enums.AutomationLevel.Automatic, Enums.AutomationLevel.Full
                    Return New AutomationConfig With {
                        .Level = level,
                        .DisplayName = "Automatic",
                        .Description = "Email received -> AI generates -> Sends directly (TrustScore >= 95%)",
                        .EnableAutoReply = True,
                        .EnableAutoCategorize = True,
                        .EnableAutoSummarize = True,
                        .EnableAutoRules = True
                    }
                Case Else
                    Return New AutomationConfig With {.Level = level}
            End Select
        End Function
    End Class

End Namespace