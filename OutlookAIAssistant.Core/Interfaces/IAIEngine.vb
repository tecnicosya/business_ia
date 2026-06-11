Namespace OutlookAIAssistant.Core.Interfaces

    ''' <summary>
    ''' Interface for AI provider engine operations: summarization, replies, translation, classification.
    ''' Supports multiple AI providers (OpenAI, DeepSeek, Gemini, Anthropic, Ollama, LM Studio, OpenRouter).
    ''' </summary>
    Public Interface IAIEngine

        ''' <summary>
        ''' Summarizes the given email content.
        ''' </summary>
        Function SummarizeAsync(request As AIRequest) As Task(Of AIResponse)

        ''' <summary>
        ''' Generates a reply draft based on the original email and optional instructions.
        ''' </summary>
        Function GenerateReplyAsync(request As AIRequest) As Task(Of AIResponse)

        ''' <summary>
        ''' Translates email content to the specified target language.
        ''' </summary>
        Function TranslateAsync(request As AIRequest, targetLanguage As String) As Task(Of AIResponse)

        ''' <summary>
        ''' Classifies the email into a predefined category.
        ''' </summary>
        Function ClassifyAsync(request As AIRequest) As Task(Of Classification)

        ''' <summary>
        ''' Proofreads and improves the grammar/style of the given text.
        ''' </summary>
        Function ProofreadAsync(request As AIRequest) As Task(Of AIResponse)

        ''' <summary>
        ''' Returns a list of configured provider names.
        ''' </summary>
        Function GetConfiguredProviders() As IReadOnlyList(Of String)

        ''' <summary>
        ''' Registers or updates an AI provider configuration.
        ''' </summary>
        Sub ConfigureProvider(config As ProviderConfig)

    End Interface

End Namespace