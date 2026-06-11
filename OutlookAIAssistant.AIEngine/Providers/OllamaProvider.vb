Imports System.Net.Http
Imports System.Text
Imports System.Threading.Tasks
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Serialization
Imports OutlookAIAssistant.Core.Models

Namespace OutlookAIAssistant.AIEngine.Providers

    ''' <summary>
    ''' Ollama on-premises provider. Communicates with local Ollama instance.
    ''' Uses OpenAI-compatible API format.
    ''' </summary>
    Public Class OllamaProvider
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
                Return "Ollama"
            End Get
        End Property

        Public Sub New(config As ProviderConfig)
            _config = config
            _httpClient = New HttpClient()
            _httpClient.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds)
        End Sub

        Public Async Function CompleteAsync(prompt As String, request As AIRequest) As Task(Of AIResponse) Implements IAIProvider.CompleteAsync
            Dim stopwatch = System.Diagnostics.Stopwatch.StartNew()

            Try
                Dim endpoint = If(String.IsNullOrEmpty(_config.ApiEndpoint),
                    "http://localhost:11434",
                    _config.ApiEndpoint.TrimEnd("/"c))

                endpoint = endpoint & "/api/generate"

                Dim model = If(String.IsNullOrEmpty(request.Model), _config.Model, request.Model)
                If String.IsNullOrEmpty(model) Then model = "llama3"

                Dim body = New With {
                    .model = model,
                    .prompt = prompt,
                    .stream = False,
                    .options = New With {
                        .num_predict = request.MaxTokens.GetValueOrDefault(_config.MaxTokens),
                        .temperature = request.Temperature.GetValueOrDefault(_config.Temperature)
                    }
                }

                Dim json = JsonConvert.SerializeObject(body, _jsonSettings)
                Dim content = New StringContent(json, Encoding.UTF8, "application/json")

                Dim response = Await _httpClient.PostAsync(endpoint, content)
                Dim responseJson = Await response.Content.ReadAsStringAsync()
                stopwatch.Stop()

                If response.IsSuccessStatusCode Then
                    Dim result = JsonConvert.DeserializeAnonymousType(responseJson, New With {
                        .model = "",
                        .response = "",
                        .total_duration = 0L,
                        .prompt_eval_count = 0,
                        .eval_count = 0
                    })

                    Return New AIResponse With {
                        .RequestId = request.RequestId,
                        .Content = result.response,
                        .ProviderName = _config.Name,
                        .ModelUsed = result.model,
                        .PromptTokens = result.prompt_eval_count,
                        .CompletionTokens = result.eval_count,
                        .TotalTokens = result.prompt_eval_count + result.eval_count,
                        .ProcessingTimeMs = result.total_duration \ 1000000,
                        .IsSuccess = True
                    }
                Else
                    Return New AIResponse With {
                        .RequestId = request.RequestId,
                        .IsSuccess = False,
                        .ErrorMessage = $"Ollama API error: {response.StatusCode} - {responseJson}",
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
                    "http://localhost:11434",
                    _config.ApiEndpoint.TrimEnd("/"c))
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