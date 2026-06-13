Imports OutlookAIAssistant.Core.Enums

Namespace OutlookAIAssistant.Core.Models

    ''' <summary>
    ''' Represents a single rule with conditions and actions.
    ''' Used by IRulesEngine and consumed throughout the Rules & Automation module.
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

        ''' <summary>Conditions to evaluate (list of typed condition objects).</summary>
        Public Property Conditions As List(Of RuleCondition)

        ''' <summary>Actions to execute if conditions match (list of typed action objects).</summary>
        Public Property Actions As List(Of RuleAction)

        ''' <summary>Priority of the rule (lower = higher priority).</summary>
        Public Property Priority As Integer = 100

        ''' <summary>Whether this rule applies only to trusted senders.</summary>
        Public Property RequiresTrustedSender As Boolean = False

        ''' <summary>
        ''' The automation level at which this rule can auto-execute.
        ''' Manual = always requires user action; AutomaticDraft = creates draft; 
        ''' Supervised = requires approval; Automatic = sends directly if trusted.
        ''' </summary>
        Public Property AutomationLevel As AutomationLevel = AutomationLevel.Manual

        ''' <summary>The logical operator for combining multiple conditions (AND / OR).</summary>
        Public Property ConditionGroupOperator As ConditionGroupOperator = ConditionGroupOperator.And

        ''' <summary>Created timestamp.</summary>
        Public Property CreatedAt As DateTime = DateTime.UtcNow

        ''' <summary>Last modified timestamp.</summary>
        Public Property ModifiedAt As DateTime = DateTime.UtcNow
    End Class

    ''' <summary>
    ''' A single condition within a rule, comparing a field against a value using an operator.
    ''' </summary>
    Public Class RuleCondition
        ''' <summary>
        ''' The email field to evaluate.
        ''' Supported values: Classification, Sender, SenderDomain, Subject, Body, 
        ''' ToRecipients, CcRecipients, HasAttachments, Categories, Priority, Sentiment.
        ''' </summary>
        Public Property Field As String

        ''' <summary>
        ''' The comparison operator.
        ''' Supported values: Equals, NotEquals, Contains, NotContains, StartsWith, 
        ''' EndsWith, GreaterThan, LessThan, In, NotIn, MatchesRegex.
        ''' </summary>
        Public Property [Operator] As String

        ''' <summary>The value to compare against.</summary>
        Public Property Value As String
    End Class

    ''' <summary>
    ''' Defines how multiple conditions in a rule are combined.
    ''' </summary>
    Public Enum ConditionGroupOperator
        ''' <summary>All conditions must match (default).</summary>
        And = 0

        ''' <summary>Any condition must match.</summary>
        Or = 1
    End Enum

    ''' <summary>
    ''' A resolved rule action to execute when conditions match.
    ''' </summary>
    Public Class RuleAction
        ''' <summary>The ID of the rule that produced this action.</summary>
        Public Property RuleId As String

        ''' <summary>
        ''' The type of action to execute.
        ''' Supported values: CreateDraft, ApplyTemplate, Forward, Flag, MoveToFolder,
        ''' MarkRead, Categorize, Delete, Reply, ReplyAll.
        ''' </summary>
        Public Property Type As String

        ''' <summary>The primary value or target for the action (e.g., folder name, template name).</summary>
        Public Property Value As String

        ''' <summary>Additional parameters for the action (key-value pairs).</summary>
        Public Property Parameters As Dictionary(Of String, String)
    End Class

    ''' <summary>
    ''' Result of processing an email through the automation pipeline.
    ''' </summary>
    Public Class PipelineResult
        ''' <summary>Whether the email was processed by any rule.</summary>
        Public Property Processed As Boolean = False

        ''' <summary>Actions that were triggered by matching rules.</summary>
        Public Property Actions As List(Of RuleAction)

        ''' <summary>Actions that were actually executed (not just suggested).</summary>
        Public Property ExecutedActions As List(Of RuleAction)

        ''' <summary>Summary text describing what happened.</summary>
        Public Property Summary As String

        ''' <summary>The trust score computed for this sender, if available.</summary>
        Public Property SenderTrustScore As TrustScore

        ''' <summary>Any errors that occurred during processing.</summary>
        Public Property Errors As List(Of String)

        ''' <summary>The automation level that was applied.</summary>
        Public Property AppliedAutomationLevel As AutomationLevel = AutomationLevel.Manual

        ''' <summary>Generated AI reply content, if applicable.</summary>
        Public Property GeneratedReply As String

        ''' <summary>Whether the generated reply was sent automatically.</summary>
        Public Property WasAutoSent As Boolean = False

        Public Sub New()
            Actions = New List(Of RuleAction)()
            ExecutedActions = New List(Of RuleAction)()
            Errors = New List(Of String)()
        End Sub
    End Class

End Namespace