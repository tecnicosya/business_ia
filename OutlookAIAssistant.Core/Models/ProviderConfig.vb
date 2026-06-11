Namespace OutlookAIAssistant.Core.Models

    ''' <summary>
    ''' Configuration for an AI provider (API endpoint, key, model, etc.).
    ''' Supports cloud providers and on-premises endpoints.
    ''' </summary>
    Public Class ProviderConfig
        ''' <summary>Display name for the provider (e.g. "OpenAI", "Ollama Local").</summary>
        Public Property Name As String

        ''' <summary>Provider type identifier (e.g. "OpenAI", "DeepSeek", "Gemini", "Anthropic", "Ollama", "LMStudio", "OpenAiCompatible").</summary>
        Public Property ProviderType As String

        ''' <summary>API base URL (for on-premises this could be http://localhost:11434).</summary>
        Public Property ApiEndpoint As String

        ''' <summary>API key (stored encrypted via DPAPIEncryption).</summary>
        Public Property ApiKey As String

        ''' <summary>Model identifier (e.g. "gpt-4o", "deepseek-chat", "claude-3-opus").</summary>
        Public Property Model As String

        ''' <summary>Default max tokens for responses.</summary>
        Public Property MaxTokens As Integer = 2048

        ''' <summary>Default temperature for generation.</summary>
        Public Property Temperature As Double = 0.7

        ''' <summary>HTTP request timeout in seconds.</summary>
        Public Property TimeoutSeconds As Integer = 60

        ''' <summary>Whether this provider is enabled.</summary>
        Public Property IsEnabled As Boolean = True

        ''' <summary>Whether this provider runs on-premises (data never leaves the user's network).</summary>
        Public Property IsOnPremises As Boolean = False

        ''' <summary>Optional custom HTTP headers (JSON format).</summary>
        Public Property CustomHeaders As String

        ''' <summary>Timestamp when this config was created.</summary>
        Public Property CreatedAt As DateTime = DateTime.UtcNow

        ''' <summary>Timestamp when this config was last modified.</summary>
        Public Property ModifiedAt As DateTime = DateTime.UtcNow
    End Class

End Namespace