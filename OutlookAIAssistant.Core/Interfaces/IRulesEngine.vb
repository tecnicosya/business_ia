Imports OutlookAIAssistant.Core.Models

Namespace OutlookAIAssistant.Core.Interfaces

    ''' <summary>
    ''' Interface for the rules and automation engine.
    ''' Evaluates conditions, triggers actions, and manages template-based replies.
    ''' </summary>
    Public Interface IRulesEngine

        ''' <summary>
        ''' Evaluates all active rules against the given email and returns actions to execute.
        ''' </summary>
        Function EvaluateAsync(email As EmailMessage) As Task(Of IReadOnlyList(Of RuleAction))

        ''' <summary>
        ''' Adds a new rule to the engine.
        ''' </summary>
        Sub AddRule(rule As Rule)

        ''' <summary>
        ''' Removes a rule by its identifier.
        ''' </summary>
        Sub RemoveRule(ruleId As String)

        ''' <summary>
        ''' Returns all registered rules.
        ''' </summary>
        Function GetRules() As IReadOnlyList(Of Rule)

        ''' <summary>
        ''' Processes an email through the full automation pipeline.
        ''' </summary>
        Function RunPipelineAsync(email As EmailMessage) As Task(Of PipelineResult)

    End Interface

End Namespace