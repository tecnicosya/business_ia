Namespace OutlookAIAssistant.Core.Models

    ''' <summary>
    ''' Represents a request to the AI engine for processing email content.
    ''' </summary>
    Public Class AIRequest
        ''' <summary>Unique identifier for this request.</summary>
        Public Property RequestId As String

        ''' <summary>The email text content to process.</summary>
        Public Property Content As String

        ''' <summary>The email subject line for context.</summary>
        Public Property Subject As String

        ''' <summary>The sender's email address for context.</summary>
        Public Property SenderEmail As String

        ''' <summary>Additional instructions or tone guidance for the AI.</summary>
        Public Property Instructions As String

        ''' <summary>Provider name to route this request to (e.g. "OpenAI", "DeepSeek").</summary>
        Public Property ProviderName As String

        ''' <summary>AI model identifier (e.g. "gpt-4o", "deepseek-chat").</summary>
        Public Property Model As String

        ''' <summary>Maximum tokens for the response.</summary>
        Public Property MaxTokens As Integer? = Nothing

        ''' <summary>Temperature for response generation (0.0 - 2.0).</summary>
        Public Property Temperature As Double? = Nothing

        ''' <summary>Whether to enable streaming responses.</summary>
        Public Property Stream As Boolean = False

        ''' <summary>The privacy mode for this request.</summary>
        Public Property PrivacyMode As PrivacyMode = PrivacyMode.Standard

        ''' <summary>Timestamp when the request was created.</summary>
        Public Property CreatedAt As DateTime = DateTime.UtcNow
    End Class

End Namespace