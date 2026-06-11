Namespace OutlookAIAssistant.AIEngine.Providers

    ''' <summary>
    ''' DeepSeek API provider. Uses OpenAI-compatible chat completions endpoint.
    ''' </summary>
    Public Class DeepSeekProvider
        Inherits OpenAiCompatibleProvider

        Public Sub New(config As ProviderConfig)
            MyBase.New(config)
        End Sub

        Public Overrides ReadOnly Property ProviderType As String
            Get
                Return "DeepSeek"
            End Get
        End Property
    End Class

End Namespace