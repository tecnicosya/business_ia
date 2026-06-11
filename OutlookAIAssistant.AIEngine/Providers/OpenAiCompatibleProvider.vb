Imports System.Net.Http
Imports System.Text
Imports System.Threading.Tasks
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Serialization
Imports OutlookAIAssistant.Core.Models

Namespace OutlookAIAssistant.AIEngine.Providers

    ''' <summary>
    ''' Generic OpenAI-compatible API provider.
    ''' Used for OpenRouter, together.ai, and any OpenAI-compatible endpoint.
    ''' Can be used as a base class for providers that share the API format.
    ''' </summary>
    Public Class OpenAiCompatibleProvider
        Implements IAIProvider

        Private ReadOnly _config As ProviderConfig
        Private ReadOnly _httpClient As HttpClient
        Private Shared ReadOnly _jsonSettings As New JsonSerializerSettings With {
            .ContractResolver = New CamelCasePropertyNamesContractResolver(),
            .NullValueHandling = NullValueHandling.Ignore
        }

        Public ReadOnly Property Name As String Implements IAIProvider.Name
            Get
                Return _config.Name
            End Get
        End Property

        Public ReadOnly Property IsEnabled As Boolean Implements IAIProvider.IsEnabled
            Get
                Return _config.IsEnabled
            End Get
        End Property

        Public Overridable ReadOnly Property ProviderType As String Implements IAIProvider.ProviderType
            Get
                Return "OpenAI Compatible"
            End Get
        End Property

        Public Sub New(config As ProviderConfig)
            _config = config
            _httpClient = New HttpClient()
            _httpClient.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds)

            ' Set API key if provided
            If Not String.IsNullOrEmpty(config.ApiKey) Then
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiKey}")
            End If
        End Sub

        Public Async Function CompleteAsync(prompt As String, request As AIRequest) As Task(Of AIResponse) Implements IAIProvider.CompleteAsync
            Dim stopwatch = System.Diagnostics.Stopwatch.StartNew()

            Try
                Dim endpoint = If(String.IsNullOrEmpty(_config.ApiEndpoint),
                    "http://localhost:1234/v1/chat/completions",
                    _config.ApiEndpoint.TrimEnd("/"c) & "/chat/completions")

                Dim model = If(String.IsNullOrEmpty(request.Model), _config.Model, request.Model)
                If String.IsNullOrEmpty(model) Then model = "default"

                Dim body = New With {
                    .model = model,
                    .messages = New Object() {
                        New With {.role = "user", .content = prompt}
                    },
                    .max_tokens = request.MaxTokens.GetValueOrDefault(_config.MaxTokens),
                    .temperature = request.Temperature.GetValueOrDefault(_config.Temperature),
                    .stream = False
                }

                Dim json = JsonConvert.SerializeObject(body, _jsonSettings)
                Dim content = New StringContent(json, Encoding.UTF8, "application/json")

                Dim response = Await _httpClient.PostAsync(endpoint, content)
                Dim responseJson = Await response.Content.ReadAsStringAsync()
                stopwatch.Stop()

                If response.IsSuccessStatusCode Then
                    Dim result = JsonConvert.DeserializeAnonymousType(responseJson, New With {
                        .id = "",
                        .model = "",
                        .usage = New With {.prompt_tokens = 0, .completion_tokens = 0, .total_tokens = 0},
                        .choices = New Object() {New With {.message = New With {.content = ""}}}
                    })

                    Dim contentText As String = ""
                    If result.choices IsNot Nothing AndAlso result.choices.Length > 0 Then
                        contentText = result.choices(0).message.content
                    End If

                    Return New AIResponse With {
                        .RequestId = request.RequestId,
                        .Content = contentText,
                        .ProviderName = _config.Name,
                        .ModelUsed = If(result.model, model),
                        .PromptTokens = result.usage.prompt_tokens,
                        .CompletionTokens = result.usage.completion_tokens,
                        .TotalTokens = result.usage.total_tokens,
                        .ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                        .IsSuccess = True
                    }
                Else
                    Return New AIResponse With {
                        .RequestId = request.RequestId,
                        .IsSuccess = False,
                        .ErrorMessage = $"API error: {response.StatusCode} - {responseJson}",
                        .ProcessingTimeMs = stopwatch.ElapsedMilliseconds
                    }
                End If

            Catch ex As Exception
                stopwatch.Stop()
                Return New AIResponse With {
                    .RequestId = request.RequestId,
                    .IsSuccess = False,
                    .ErrorMessage = ex.Message,
                    .ProcessingTimeMs = stopwatch.ElapsedMilliseconds
                }
            End Try
        End Function

        Public Async Function CompleteStreamAsync(prompt As String, request As AIRequest, onChunk As Action(Of String)) As Task(Of AIResponse) Implements IAIProvider.CompleteStreamAsync
            Return Await CompleteAsync(prompt, request)
        End Function

        Public Async Function TestConnectionAsync() As Task(Of Boolean) Implements IAIProvider.TestConnectionAsync
            Try
                Dim endpoint = If(String.IsNullOrEmpty(_config.ApiEndpoint),
                    "http://localhost:1234/v1/models",
                    _config.ApiEndpoint.TrimEnd("/"c) & "/models")
                Dim response = Await _httpClient.GetAsync(endpoint)
                Return response.IsSuccessStatusCode
            Catch
                Return False
            End Try
        End Function

        Protected Overrides Sub Finalize()
            _httpClient?.Dispose()
            MyBase.Finalize()
        End Sub
    End Class

End Namespace