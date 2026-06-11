Namespace OutlookAIAssistant.Core.Interfaces

    ''' <summary>
    ''' Interface for licensing — validates license keys, enforces plan limits, manages activation.
    ''' Supports Free, Personal, Pro, and Business plans.
    ''' </summary>
    Public Interface ILicensingService

        ''' <summary>
        ''' Activates a license key. Returns True if activation succeeds.
        ''' </summary>
        Function ActivateLicenseAsync(licenseKey As String) As Task(Of Boolean)

        ''' <summary>
        ''' Returns the current active plan.
        ''' </summary>
        Function GetCurrentPlan() As LicensingPlan

        ''' <summary>
        ''' Returns remaining AI operations for the current period.
        ''' </summary>
        Function GetRemainingOperations() As Integer

        ''' <summary>
        ''' Returns True if the current plan/usage allows an AI operation.
        ''' </summary>
        Function CanPerformOperation() As Boolean

        ''' <summary>
        ''' Decrements the operation counter after an AI call.
        ''' </summary>
        Sub RecordOperation()

        ''' <summary>
        ''' Returns the expiration date of the current license.
        ''' </summary>
        Function GetExpirationDate() As Date?

    End Interface

    ''' <summary>
    ''' Available licensing plans.
    ''' </summary>
    Public Enum LicensingPlan
        Free
        Personal
        Pro
        Business
    End Enum

End Namespace