Imports System.Threading.Tasks
Imports OutlookAIAssistant.Core.Interfaces
Imports OutlookAIAssistant.Core.Models

Namespace OutlookAIAssistant.Rules

    ''' <summary>
    ''' Represents a single rule with conditions and actions.
    ''' </summary>
    Public Class Rule
        ''' <summary>Unique rule identifier.</summary>
        Public Property Id As String

        ''' <summary>Display name for the rule.</summary>
        Public Property Name As String

        ''' <summary>Description of what the rule does.</summary>
        Public Property Description As String

        ''' <summary>Whether the rule is enabled.</summary>
        Public Property IsEnabled As Boolean = True

        ''' <summary>Conditions to evaluate (JSON format for flexibility).</summary>
        Public Property Conditions As String

        ''' <summary>Actions to execute if conditions match (JSON format).</summary>
        Public Property Actions As String

        ''' <summary>Priority of the rule (lower = higher priority).</summary>
        Public Property Priority As Integer = 100

        ''' <summary>Whether this rule applies only to trusted senders.</summary>
        Public Property RequiresTrustedSender As Boolean = False

        ''' <summary>Created timestamp.</summary>
        Public Property CreatedAt As DateTime = DateTime.UtcNow

        ''' <summary>Last modified timestamp.</summary>
        Public Property ModifiedAt As DateTime = DateTime.UtcNow
    End Class

    ''' <summary>
    ''' A resolved rule action to execute.
    ''' </summary>
    Public Class RuleAction
        Public Property RuleId As String
        Public Property Type As String ' "move", "categorize", "reply", "forward", "flag", "delete", "markRead"
        Public Property Value As String
        Public Property Parameters As Dictionary(Of String, String)
    End Class

    ''' <summary>
    ''' Result of running the automation pipeline.
    ''' </summary>
    Public Class PipelineResult
        Public Property Processed As Boolean = False
        Public Property Actions As List(Of RuleAction)
        Public Property Summary As String
        Public Property Errors As List(Of String)
    End Class

End Namespace