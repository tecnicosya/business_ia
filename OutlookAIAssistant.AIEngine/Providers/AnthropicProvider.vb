Imports System.Net.Http
Imports System.Text
Imports System.Threading.Tasks
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Serialization
Imports OutlookAIAssistant.Core.Models

Namespace OutlookAIAssistant.AIEngine.Providers

    ''' <summary>
    ''' Anthropic Claude API provider.
    ''' </summary>
    Public Class AnthropicProvider
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
                Return "Anthropic"
            End Get
        End Property

        Public Sub New(config As ProviderConfig)
            _config = config
            _httpClient = New HttpClient()
            _httpClient.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds)
            _httpClient.DefaultRequestHeaders.Add("x-api-key", config.ApiKey)
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01")
        End Sub

        Public Async Function CompleteAsync(prompt As String, request As AIRequest) As Task(Of AIResponse) Implements IAIProvider.CompleteAsync
            Dim stopwatch = System.Diagnostics.Stopwatch.StartNew()

            Try
                Dim endpoint = If(String.IsNullOrEmpty(_config.ApiEndpoint),
                    "https://api.anthropic.com/v1/messages",
                    _config.ApiEndpoint.TrimEnd("/"c) & "/v1/messages")

                Dim model = If(String.IsNullOrEmpty(request.Model), _config.Model, request.Model)

                Dim body = New With {
                    .model = model,
                    .max_tokens = request.MaxTokens.GetValueOrDefault(_config.MaxTokens),
                    .messages = New Object() {
                        New With {.role = "user", .content = prompt}
                    }
                }

                ' Only include temperature if explicitly set
                If request.Temperature.HasValue Then
                    body = New With {
                        .model = model,
                        .max_tokens = request.MaxTokens.GetValueOrDefault(_config.MaxTokens),
                        .temperature = request.Temperature.Value,
                        .messages = New Object() {
                            New With {.role = "user", .content = prompt}
                        }
                    }
                End If

                Dim json = JsonConvert.SerializeObject(body, _jsonSettings)
                Dim content = New StringContent(json, Encoding.UTF8, "application/json")

                Dim response = Await _httpClient.PostAsync(endpoint, content)
                Dim responseJson = Await response.Content.ReadAsStringAsync()
                stopwatch.Stop()

                If response.IsSuccessStatusCode Then
                    Dim result = JsonConvert.DeserializeAnonymousType(responseJson, New With {
                        .id = "",
                        .model = "",
                        .usage = New With {.input_tokens = 0, .output_tokens = 0},
                        .content = New Object() {New With {.text = ""}}
                    })

                    Return New AIResponse With {
                        .RequestId = request.RequestId,
                        .Content = result.content(0).text,
                        .ProviderName = _config.Name,
                        .ModelUsed = result.model,
                        .PromptTokens = result.usage.input_tokens,
                        .CompletionTokens = result.usage.output_tokens,
                        .TotalTokens = result.usage.input_tokens + result.usage.output_tokens,
                        .ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                        .IsSuccess = True
                    }
                Else
                    Return New AIResponse With {
                        .RequestId = request.RequestId,
                        .IsSuccess = False,
                        .ErrorMessage = $"Anthropic API error: {response.StatusCode} - {responseJson}",
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
                Dim response = Await _httpClient.GetAsync("https://api.anthropic.com/v1/models")
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