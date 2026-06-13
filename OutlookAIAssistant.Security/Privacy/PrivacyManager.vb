Imports OutlookAIAssistant.Core.Models

Namespace OutlookAIAssistant.Security.Privacy

    ''' <summary>
    ''' Manages privacy configurations and user data protection.
    ''' Enforces the selected privacy mode (Standard, Anonymized, or On-Premises),
    ''' validates provider compatibility, and warns when switching modes.
    ''' </summary>
    Public Class PrivacyManager

        Private _currentPrivacyMode As PrivacyMode
        Private _previousPrivacyMode As PrivacyMode

        Public Sub New()
            _currentPrivacyMode = PrivacyMode.Standard
            _previousPrivacyMode = PrivacyMode.Standard
        End Sub

        ''' <summary>
        ''' Gets or sets the current privacy mode.
        ''' Setting a new mode triggers validation and stores the previous mode.
        ''' </summary>
        Public Property CurrentPrivacyMode As PrivacyMode
            Get
                Return _currentPrivacyMode
            End Get
            Set(value As PrivacyMode)
                If value Is Nothing Then
                    Throw New ArgumentNullException(NameOf(value), "Privacy mode cannot be Nothing.")
                End If

                _previousPrivacyMode = _currentPrivacyMode
                _currentPrivacyMode = value
            End Set
        End Property

        ''' <summary>
        ''' Returns the previous privacy mode, useful for undo or confirmation dialogs.
        ''' </summary>
        Public ReadOnly Property PreviousPrivacyMode As PrivacyMode
            Get
                Return _previousPrivacyMode
            End Get
        End Property

        ''' <summary>
        ''' Returns whether PII anonymization is enabled.
        ''' </summary>
        Public ReadOnly Property IsAnonymizePII As Boolean
            Get
                Return _currentPrivacyMode.AnonymizePII
            End Get
        End Property

        ''' <summary>
        ''' Returns whether on-premises processing is enforced (no data leaves the machine).
        ''' </summary>
        Public ReadOnly Property IsOnPremisesOnly As Boolean
            Get
                Return _currentPrivacyMode.OnPremises
            End Get
        End Property

        ''' <summary>
        ''' Returns whether content is processed and stored locally only.
        ''' </summary>
        Public ReadOnly Property IsLocalOnly As Boolean
            Get
                Return _currentPrivacyMode.LocalOnly
            End Get
        End Property

        ''' <summary>
        ''' Returns whether analytics collection is allowed.
        ''' </summary>
        Public ReadOnly Property IsAllowAnalytics As Boolean
            Get
                Return _currentPrivacyMode.AllowAnalytics
            End Get
        End Property

        ''' <summary>
        ''' Returns whether local caching is enabled.
        ''' </summary>
        Public ReadOnly Property IsLocalCachingEnabled As Boolean
            Get
                Return _currentPrivacyMode.EnableLocalCaching
            End Get
        End Property

        ''' <summary>
        ''' Returns the cache duration in hours for the current mode.
        ''' </summary>
        Public ReadOnly Property CacheDurationHours As Integer
            Get
                Return _currentPrivacyMode.CacheDurationHours
            End Get
        End Property

        ''' <summary>
        ''' Validates whether a given provider type is allowed under current privacy settings.
        ''' In On-Premises mode, only local providers (Ollama, LM Studio) are allowed.
        ''' </summary>
        Public Function IsProviderAllowed(providerType As String, isOnPremises As Boolean) As Boolean
            ' On-Premises mode: only local/on-premises providers
            If _currentPrivacyMode.OnPremises AndAlso Not isOnPremises Then
                Return False
            End If

            ' Local-only: only local providers
            If _currentPrivacyMode.LocalOnly AndAlso Not isOnPremises Then
                Return False
            End If

            Return True
        End Function

        ''' <summary>
        ''' Determines if switching from the current mode to a target mode
        ''' would cause data to be handled differently (e.g. from On-Premises to Cloud).
        ''' Returns True if the switch would reduce privacy protections.
        ''' </summary>
        Public Function WouldReducePrivacy(targetMode As PrivacyMode) As Boolean
            If targetMode Is Nothing Then Return False

            ' Switching from On-Premises to anything else reduces privacy
            If _currentPrivacyMode.OnPremises AndAlso Not targetMode.OnPremises Then
                Return True
            End If

            ' Switching from Anonymized to Standard reduces privacy
            If _currentPrivacyMode.AnonymizePII AndAlso Not targetMode.AnonymizePII Then
                Return True
            End If

            Return False
        End Function

        ''' <summary>
        ''' Returns a human-readable warning message when switching to a less-private mode.
        ''' Returns empty string if no warning is needed.
        ''' </summary>
        Public Function GetPrivacyWarning(targetMode As PrivacyMode) As String
            If targetMode Is Nothing Then Return String.Empty

            If _currentPrivacyMode.OnPremises AndAlso Not targetMode.OnPremises Then
                Return "Warning: Switching from On-Premises to cloud processing will send email content to external AI providers. " &
                       "This may expose sensitive data. Are you sure you want to continue?"
            End If

            If _currentPrivacyMode.AnonymizePII AndAlso Not targetMode.AnonymizePII Then
                Return "Warning: Switching from Anonymized to Standard mode means personally identifiable information (PII) " &
                       "will be sent to AI providers without redaction. Are you sure you want to continue?"
            End If

            Return String.Empty
        End Function

        ''' <summary>
        ''' Returns a human-readable description of the current privacy setting.
        ''' </summary>
        Public Function GetPrivacyDescription() As String
            Select Case _currentPrivacyMode.Name
                Case "Standard"
                    Return "Standard mode: Email content is sent to cloud AI providers for processing. " &
                           "Anonymized analytics are collected to improve the product."
                Case "Anonymized"
                    Return "Anonymized mode: Personally identifiable information (names, emails, phone numbers, addresses) " &
                           "is stripped from email content before sending to cloud providers. Analytics are collected."
                Case "On-Premises"
                    Return "On-premises mode: All processing happens locally via Ollama or LM Studio. " &
                           "No email data leaves your computer. No analytics are collected."
                Case Else
                    Return "Standard mode: Email content is sent to cloud AI providers."
            End Select
        End Function

        ''' <summary>
        ''' Resets privacy mode to Standard.
        ''' </summary>
        Public Sub ResetToDefault()
            _previousPrivacyMode = _currentPrivacyMode
            _currentPrivacyMode = PrivacyMode.Standard
        End Sub

    End Class

End Namespace