Namespace OutlookAIAssistant.Core.Models

    ''' <summary>
    ''' Represents a response from the AI engine.
    ''' </summary>
    Public Class AIResponse
        ''' <summary>Unique identifier matching the original request.</summary>
        Public Property RequestId As String

        ''' <summary>The generated text content.</summary>
        Public Property Content As String

        ''' <summary>The provider that fulfilled this request.</summary>
        Public Property ProviderName As String

        ''' <summary>The model used to generate the response.</summary>
        Public Property ModelUsed As String

        ''' <summary>Tokens used for the input prompt.</summary>
        Public Property PromptTokens As Integer

        ''' <summary>Tokens used for the completion.</summary>
        Public Property CompletionTokens As Integer

        ''' <summary>Total tokens consumed.</summary>
        Public Property TotalTokens As Integer

        ''' <summary>Processing duration in milliseconds.</summary>
        Public Property ProcessingTimeMs As Long

        ''' <summary>Whether the response completed successfully.</summary>
        Public Property IsSuccess As Boolean = True

        ''' <summary>Error message if the request failed.</summary>
        Public Property ErrorMessage As String

        ''' <summary>Timestamp when the response was generated.</summary>
        Public Property CreatedAt As DateTime = DateTime.UtcNow
    End Class

End Namespace