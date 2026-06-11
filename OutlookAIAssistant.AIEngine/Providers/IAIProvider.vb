Imports System.Threading.Tasks
Imports OutlookAIAssistant.Core.Models

Namespace OutlookAIAssistant.AIEngine.Providers

    ''' <summary>
    ''' Interface for individual AI provider implementations.
    ''' Each provider wraps a specific API (OpenAI, DeepSeek, Gemini, etc.).
    ''' </summary>
    Public Interface IAIProvider
        ''' <summary>Provider display name.</summary>
        ReadOnly Property Name As String

        ''' <summary>Whether this provider is enabled and ready.</summary>
        ReadOnly Property IsEnabled As Boolean

        ''' <summary>The provider type identifier.</summary>
        ReadOnly Property ProviderType As String

        ''' <summary>Sends a completion request and returns the response.</summary>
        Function CompleteAsync(prompt As String, request As AIRequest) As Task(Of AIResponse)

        ''' <summary>Sends a streaming completion request.</summary>
        Function CompleteStreamAsync(prompt As String, request As AIRequest, onChunk As Action(Of String)) As Task(Of AIResponse)

        ''' <summary>Tests the provider connection.</summary>
        Function TestConnectionAsync() As Task(Of Boolean)
    End Interface

End Namespace