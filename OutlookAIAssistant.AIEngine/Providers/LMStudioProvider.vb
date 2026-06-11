Namespace OutlookAIAssistant.AIEngine.Providers

    ''' <summary>
    ''' LM Studio on-premises provider. Uses OpenAI-compatible API format.
    ''' Typically runs on http://localhost:1234.
    ''' </summary>
    Public Class LMStudioProvider
        Inherits OpenAiCompatibleProvider

        Public Sub New(config As ProviderConfig)
            MyBase.New(config)
        End Sub

        Public Overrides ReadOnly Property ProviderType As String
            Get
                Return "LM Studio"
            End Get
        End Property
    End Class

End Namespace