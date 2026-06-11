Namespace OutlookAIAssistant.Core.Models

    ''' <summary>
    ''' Describes how the AI engine handles email data privacy.
    ''' </summary>
    Public Class PrivacyMode
        ''' <summary>Mode name: Standard, Anonymized, OnPremises, LocalOnly.</summary>
        Public Property Name As String

        ''' <summary>Whether PII (personally identifiable information) is stripped before sending.</summary>
        Public Property AnonymizePII As Boolean = False

        ''' <summary>Whether content is processed on-premises.</summary>
        Public Property OnPremises As Boolean = False

        ''' <summary>Whether data is stored locally only.</summary>
        Public Property LocalOnly As Boolean = False

        ''' <summary>Whether analytics data (anonymized) can be collected.</summary>
        Public Property AllowAnalytics As Boolean = True

        ''' <summary>Whether email content is cached locally.</summary>
        Public Property EnableLocalCaching As Boolean = True

        ''' <summary>Cache duration in hours for local cache.</summary>
        Public Property CacheDurationHours As Integer = 24

        ''' <summary>Standard mode — normal API processing with caching.</summary>
        Public Shared ReadOnly Property Standard As PrivacyMode
            Get
                Return New PrivacyMode With {
                    .Name = "Standard",
                    .AnonymizePII = False,
                    .OnPremises = False,
                    .LocalOnly = False,
                    .AllowAnalytics = True,
                    .EnableLocalCaching = True,
                    .CacheDurationHours = 24
                }
            End Get
        End Property

        ''' <summary>Anonymized mode — PII stripped before sending to API.</summary>
        Public Shared ReadOnly Property Anonymized As PrivacyMode
            Get
                Return New PrivacyMode With {
                    .Name = "Anonymized",
                    .AnonymizePII = True,
                    .OnPremises = False,
                    .LocalOnly = False,
                    .AllowAnalytics = True,
                    .EnableLocalCaching = True,
                    .CacheDurationHours = 24
                }
            End Get
        End Property

        ''' <summary>On-premises mode — processing via local AI (Ollama/LM Studio).</summary>
        Public Shared ReadOnly Property OnPremisesMode As PrivacyMode
            Get
                Return New PrivacyMode With {
                    .Name = "On-Premises",
                    .AnonymizePII = False,
                    .OnPremises = True,
                    .LocalOnly = True,
                    .AllowAnalytics = False,
                    .EnableLocalCaching = True,
                    .CacheDurationHours = 48
                }
            End Get
        End Property
    End Class

End Namespace