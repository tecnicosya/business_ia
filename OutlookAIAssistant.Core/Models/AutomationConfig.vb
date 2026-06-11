Imports OutlookAIAssistant.Core.Enums

Namespace OutlookAIAssistant.Core.Models

    ''' <summary>
    ''' Configuration for an automation level, wrapping the enum with additional metadata.
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
                        .Description = "Everything requires manual approval",
                        .EnableAutoReply = False,
                        .EnableAutoCategorize = False,
                        .EnableAutoSummarize = False,
                        .EnableAutoRules = False
                    }
                Case Enums.AutomationLevel.Suggestions
                    Return New AutomationConfig With {
                        .Level = level,
                        .DisplayName = "Suggestions",
                        .Description = "AI suggests actions, user approves",
                        .EnableAutoReply = False,
                        .EnableAutoCategorize = True,
                        .EnableAutoSummarize = True,
                        .EnableAutoRules = False
                    }
                Case Enums.AutomationLevel.Partial
                    Return New AutomationConfig With {
                        .Level = level,
                        .DisplayName = "Partial Automation",
                        .Description = "Auto-categorize and summarize known senders",
                        .EnableAutoReply = False,
                        .EnableAutoCategorize = True,
                        .EnableAutoSummarize = True,
                        .EnableAutoRules = True
                    }
                Case Enums.AutomationLevel.Full
                    Return New AutomationConfig With {
                        .Level = level,
                        .DisplayName = "Full Automation",
                        .Description = "Fully automated processing with AI replies",
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