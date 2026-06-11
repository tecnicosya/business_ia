Imports System.Net.Http
Imports System.Text
Imports System.Threading.Tasks
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Serialization
Imports OutlookAIAssistant.Core.Models

Namespace OutlookAIAssistant.AIEngine.Providers

    ''' <summary>
    ''' Google Gemini API provider. Uses the Gemini generative endpoint.
    ''' </summary>
    Public Class GeminiProvider
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
                Return "Gemini"
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
                Dim model = If(String.IsNullOrEmpty(request.Model), _config.Model, request.Model)
                Dim baseUrl = If(String.IsNullOrEmpty(_config.ApiEndpoint),
                    "https://generativelanguage.googleapis.com",
                    _config.ApiEndpoint.TrimEnd("/"c))

                Dim endpoint = $"{baseUrl}/v1beta/models/{model}:generateContent?key={_config.ApiKey}"

                Dim body = New With {
                    .contents = New Object() {
                        New With {.parts = New Object() {New With {.text = prompt}}}
                    },
                    .generationConfig = New With {
                        .maxOutputTokens = request.MaxTokens.GetValueOrDefault(_config.MaxTokens),
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
                        .candidates = New Object() {New With {
                            .content = New With {.parts = New Object() {New With {.text = ""}}}
                        }}
                    })

                    Return New AIResponse With {
                        .RequestId = request.RequestId,
                        .Content = result.candidates(0).content.parts(0).text,
                        .ProviderName = _config.Name,
                        .ModelUsed = model,
                        .ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                        .IsSuccess = True
                    }
                Else
                    Return New AIResponse With {
                        .RequestId = request.RequestId,
                        .IsSuccess = False,
                        .ErrorMessage = $"Gemini API error: {response.StatusCode} - {responseJson}",
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
                Dim model = If(String.IsNullOrEmpty(_config.Model), "gemini-pro", _config.Model)
                Dim url = $"https://generativelanguage.googleapis.com/v1beta/models?key={_config.ApiKey}"
                Dim response = Await _httpClient.GetAsync(url)
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