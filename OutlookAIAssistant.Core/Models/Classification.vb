Imports OutlookAIAssistant.Core.Enums

Namespace OutlookAIAssistant.Core.Models

    ''' <summary>
    ''' Represents the classification result of an email by the AI engine.
    ''' </summary>
    Public Class Classification
        ''' <summary>The primary classification type.</summary>
        Public Property Type As ClassificationType

        ''' <summary>Confidence score (0.0 - 1.0).</summary>
        Public Property Confidence As Double

        ''' <summary>Specific subcategory or tag (e.g. "invoice", "bug-report", "meeting-request").</summary>
        Public Property SubCategory As String

        ''' <summary>Suggested priority level (e.g. "high", "normal", "low").</summary>
        Public Property Priority As String

        ''' <summary>Whether this email requires immediate action.</summary>
        Public Property RequiresAction As Boolean

        ''' <summary>Suggested sentiment (e.g. "positive", "neutral", "negative", "urgent").</summary>
        Public Property Sentiment As String

        ''' <summary>Auto-detected language of the email.</summary>
        Public Property DetectedLanguage As String

        ''' <summary>Optional reason or explanation for this classification.</summary>
        Public Property Explanation As String
    End Class

End Namespace