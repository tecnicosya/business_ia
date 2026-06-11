Imports OutlookAIAssistant.Core.Models

Namespace OutlookAIAssistant.Security.Privacy

    ''' <summary>
    ''' Manages privacy configurations and user data protection.
    ''' Ensures compliance with privacy requirements and PII handling.
    ''' </summary>
    Public Class PrivacyManager

        Private _currentPrivacyMode As PrivacyMode
        Private _anonymizePII As Boolean = False
        Private _onPremisesOnly As Boolean = False
        Private _allowAnalytics As Boolean = True

        Public Sub New()
            _currentPrivacyMode = PrivacyMode.Standard
        End Sub

        ''' <summary>
        ''' Gets or sets the current privacy mode.
        ''' </summary>
        Public Property CurrentPrivacyMode As PrivacyMode
            Get
                Return _currentPrivacyMode
            End Get
            Set(value As PrivacyMode)
                _currentPrivacyMode = value
                ApplyPrivacySettings(value)
            End Set
        End Property

        ''' <summary>
        ''' Returns whether PII anonymization is enabled.
        ''' </summary>
        Public ReadOnly Property IsAnonymizePII As Boolean
            Get
                Return _anonymizePII
            End Get
        End Property

        ''' <summary>
        ''' Returns whether on-premises processing is enforced.
        ''' </summary>
        Public ReadOnly Property IsOnPremisesOnly As Boolean
            Get
                Return _onPremisesOnly
            End Get
        End Property

        ''' <summary>
        ''' Returns whether analytics collection is allowed.
        ''' </summary>
        Public ReadOnly Property IsAllowAnalytics As Boolean
            Get
                Return _allowAnalytics
            End Get
        End Property

        Private Sub ApplyPrivacySettings(mode As PrivacyMode)
            _anonymizePII = mode.AnonymizePII
            _onPremisesOnly = mode.OnPremises
            _allowAnalytics = mode.AllowAnalytics
        End Sub

        ''' <summary>
        ''' Validates whether a given provider type is allowed under current privacy settings.
        ''' </summary>
        Public Function IsProviderAllowed(providerType As String, isOnPremises As Boolean) As Boolean
            If _onPremisesOnly AndAlso Not isOnPremises Then
                Return False ' On-premises mode: only local providers
            End If
            Return True
        End Function

        ''' <summary>
        ''' Returns a human-readable description of the current privacy setting.
        ''' </summary>
        Public Function GetPrivacyDescription() As String
            Select Case _currentPrivacyMode.Name
                Case "Standard"
                    Return "Standard mode: Email content is sent to cloud AI providers. Analytics are collected."
                Case "Anonymized"
                    Return "Anonymized mode: PII is stripped before sending to cloud providers. Analytics are collected."
                Case "On-Premises"
                    Return "On-premises mode: All processing happens locally. No data leaves your network. No analytics."
                Case Else
                    Return "Standard mode"
            End Select
        End Function

    End Class

End Namespace