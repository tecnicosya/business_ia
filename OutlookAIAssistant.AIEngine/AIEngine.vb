Imports System.Threading.Tasks
Imports OutlookAIAssistant.Core.Interfaces
Imports OutlookAIAssistant.Core.Models

Namespace OutlookAIAssistant.AIEngine

    ''' <summary>
    ''' Main AI engine that orchestrates multiple AI providers.
    ''' Implements IAIEngine and routes requests to configured providers.
    ''' </summary>
    Public Class AIEngine
        Implements IAIEngine

        Private ReadOnly _providers As New Dictionary(Of String, Providers.IAIProvider)(StringComparer.OrdinalIgnoreCase)
        Private ReadOnly _cache As Cache.EmailCache
        Private ReadOnly _privacyFilter As Privacy.PrivacyFilter
        Private ReadOnly _trustScorer As Scoring.TrustScorer
        Private ReadOnly _promptManager As Prompts.PromptManager
        Private ReadOnly _lock As New Object()

        Public Sub New()
            _cache = New Cache.EmailCache()
            _privacyFilter = New Privacy.PrivacyFilter()
            _trustScorer = New Scoring.TrustScorer()
            _promptManager = New Prompts.PromptManager()
        End Sub

        Public Async Function SummarizeAsync(request As AIRequest) As Task(Of AIResponse) Implements IAIEngine.SummarizeAsync
            Dim cacheKey = $"summarize:{HashContent(request.Content)}"

            ' Check cache first
            Dim cached = _cache.TryGetValue(Of AIResponse)(cacheKey)
            If cached IsNot Nothing Then Return cached

            ' Apply privacy filter
            Dim filteredContent = _privacyFilter.Apply(request.Content, request.PrivacyMode)

            ' Build prompt
            Dim prompt = _promptManager.BuildSummaryPrompt(filteredContent, request.Subject)

            ' Send to provider
            Dim provider = GetProvider(request.ProviderName)
            Dim response = Await provider.CompleteAsync(prompt, request)

            ' Cache result
            If response.IsSuccess Then
                _cache.SetValue(cacheKey, response, TimeSpan.FromHours(1))
            End If

            Return response
        End Function

        Public Async Function GenerateReplyAsync(request As AIRequest) As Task(Of AIResponse) Implements IAIEngine.GenerateReplyAsync
            Dim cacheKey = $"reply:{HashContent(request.Content)}"
            Dim cached = _cache.TryGetValue(Of AIResponse)(cacheKey)
            If cached IsNot Nothing Then Return cached

            Dim filteredContent = _privacyFilter.Apply(request.Content, request.PrivacyMode)
            Dim prompt = _promptManager.BuildReplyPrompt(filteredContent, request.Subject, request.SenderEmail, request.Instructions)
            Dim provider = GetProvider(request.ProviderName)
            Dim response = Await provider.CompleteAsync(prompt, request)

            If response.IsSuccess Then
                _cache.SetValue(cacheKey, response, TimeSpan.FromMinutes(30))
            End If

            Return response
        End Function

        Public Async Function TranslateAsync(request As AIRequest, targetLanguage As String) As Task(Of AIResponse) Implements IAIEngine.TranslateAsync
            Dim cacheKey = $"translate:{targetLanguage}:{HashContent(request.Content)}"
            Dim cached = _cache.TryGetValue(Of AIResponse)(cacheKey)
            If cached IsNot Nothing Then Return cached

            Dim filteredContent = _privacyFilter.Apply(request.Content, request.PrivacyMode)
            Dim prompt = _promptManager.BuildTranslationPrompt(filteredContent, targetLanguage)
            Dim provider = GetProvider(request.ProviderName)
            Dim response = Await provider.CompleteAsync(prompt, request)

            If response.IsSuccess Then
                _cache.SetValue(cacheKey, response, TimeSpan.FromHours(2))
            End If

            Return response
        End Function

        Public Async Function ClassifyAsync(request As AIRequest) As Task(Of Classification) Implements IAIEngine.ClassifyAsync
            Dim cacheKey = $"classify:{HashContent(request.Content)}"
            Dim cached = _cache.TryGetValue(Of Classification)(cacheKey)
            If cached IsNot Nothing Then Return cached

            Dim filteredContent = _privacyFilter.Apply(request.Content, request.PrivacyMode)
            Dim prompt = _promptManager.BuildClassificationPrompt(filteredContent, request.Subject)
            Dim provider = GetProvider(request.ProviderName)
            Dim response = Await provider.CompleteAsync(prompt, request)

            Dim classification As Classification = Nothing
            If response.IsSuccess Then
                classification = ParseClassification(response.Content)
                _cache.SetValue(cacheKey, classification, TimeSpan.FromHours(4))
            End If

            Return classification
        End Function

        Public Async Function ProofreadAsync(request As AIRequest) As Task(Of AIResponse) Implements IAIEngine.ProofreadAsync
            Dim filteredContent = _privacyFilter.Apply(request.Content, request.PrivacyMode)
            Dim prompt = _promptManager.BuildProofreadPrompt(filteredContent)
            Dim provider = GetProvider(request.ProviderName)
            Return Await provider.CompleteAsync(prompt, request)
        End Function

        Public Function GetConfiguredProviders() As IReadOnlyList(Of String) Implements IAIEngine.GetConfiguredProviders
            SyncLock _lock
                Return _providers.Keys.Where(Function(k) _providers(k).IsEnabled).ToList().AsReadOnly()
            End SyncLock
        End Function

        Public Sub ConfigureProvider(config As ProviderConfig) Implements IAIEngine.ConfigureProvider
            Dim provider = CreateProvider(config)
            SyncLock _lock
                If _providers.ContainsKey(config.Name) Then
                    _providers(config.Name) = provider
                Else
                    _providers.Add(config.Name, provider)
                End If
            End SyncLock
        End Sub

        Private Function GetProvider(name As String) As Providers.IAIProvider
            SyncLock _lock
                If _providers.TryGetValue(name, Dim provider) AndAlso provider.IsEnabled Then
                    Return provider
                End If
            End SyncLock
            Throw New InvalidOperationException($"Provider '{name}' is not configured or is disabled.")
        End Function

        Private Function CreateProvider(config As ProviderConfig) As Providers.IAIProvider
            Select Case config.ProviderType.ToLowerInvariant()
                Case "openai"
                    Return New Providers.OpenAIProvider(config)
                Case "deepseek"
                    Return New Providers.DeepSeekProvider(config)
                Case "gemini"
                    Return New Providers.GeminiProvider(config)
                Case "anthropic"
                    Return New Providers.AnthropicProvider(config)
                Case "ollama"
                    Return New Providers.OllamaProvider(config)
                Case "lmstudio", "lm studio"
                    Return New Providers.LMStudioProvider(config)
                Case "openrouter", "openaicompatible"
                    Return New Providers.OpenAiCompatibleProvider(config)
                Case Else
                    Throw New ArgumentException($"Unsupported provider type: {config.ProviderType}")
            End Select
        End Function

        Private Function HashContent(content As String) As String
            Using sha = System.Security.Cryptography.SHA256.Create()
                Dim bytes = System.Text.Encoding.UTF8.GetBytes(content)
                Dim hash = sha.ComputeHash(bytes)
                Return BitConverter.ToString(hash).Replace("-", "").Substring(0, 32)
            End Using
        End Function

        Private Function ParseClassification(json As String) As Classification
            ' Simplified JSON parsing — production should use Newtonsoft.Json
            Try
                Return Newtonsoft.Json.JsonConvert.DeserializeObject(Of Classification)(json)
            Catch
                Return New Classification With {
                    .Type = Enums.ClassificationType.Unknown,
                    .Confidence = 0.0
                }
            End Try
        End Function

    End Class

End Namespace