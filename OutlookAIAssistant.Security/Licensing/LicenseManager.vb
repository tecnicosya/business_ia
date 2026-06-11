Imports OutlookAIAssistant.Core.Interfaces

Namespace OutlookAIAssistant.Security.Licensing

    ''' <summary>
    ''' Manages product licensing and plan enforcement.
    ''' Validates license keys, tracks usage, and enforces plan limits.
    ''' Supports Free, Personal ($49/yr), Pro ($99/yr), and Business ($299+/yr).
    ''' </summary>
    Public Class LicenseManager
        Implements ILicensingService

        Private _currentPlan As LicensingPlan = LicensingPlan.Free
        Private _licenseKey As String
        Private _expirationDate As Date?
        Private _operationsRemaining As Integer = 10 ' Free plan: 10/day
        Private _lastResetDate As Date = Date.Today

        Public ReadOnly Property CurrentPlan As LicensingPlan
            Get
                Return _currentPlan
            End Get
        End Property

        Public Async Function ActivateLicenseAsync(licenseKey As String) As Task(Of Boolean) Implements ILicensingService.ActivateLicenseAsync
            _licenseKey = licenseKey

            ' In production: validate against licensing server
            ' For now, simulate async validation
            Dim isValid = Await Task.Run(Function() ValidateLicense(licenseKey))

            If isValid Then
                ' Parse plan from license key
                _currentPlan = DeterminePlan(licenseKey)
                UpdatePlanLimits()
            End If

            Return isValid
        End Function

        Public Function GetCurrentPlan() As LicensingPlan Implements ILicensingService.GetCurrentPlan
            Return _currentPlan
        End Function

        Public Function GetRemainingOperations() As Integer Implements ILicensingService.GetRemainingOperations
            ResetDailyIfNeeded()
            Return _operationsRemaining
        End Function

        Public Function CanPerformOperation() As Boolean Implements ILicensingService.CanPerformOperation
            ResetDailyIfNeeded()
            Return _operationsRemaining > 0
        End Function

        Public Sub RecordOperation() Implements ILicensingService.RecordOperation
            ResetDailyIfNeeded()
            If _operationsRemaining > 0 Then
                _operationsRemaining -= 1
            End If
        End Sub

        Public Function GetExpirationDate() As Date? Implements ILicensingService.GetExpirationDate
            Return _expirationDate
        End Function

        ''' <summary>
        ''' Returns the plan display name and limits.
        ''' </summary>
        Public Function GetPlanInfo() As (Name As String, DailyLimit As Integer, MonthlyLimit As Integer, Price As String)
            Select Case _currentPlan
                Case LicensingPlan.Free
                    Return ("Free", 10, 100, "$0")
                Case LicensingPlan.Personal
                    Return ("Personal", 100, 3000, "$49/yr")
                Case LicensingPlan.Pro
                    Return ("Pro", Unlimited, Unlimited, "$99/yr")
                Case LicensingPlan.Business
                    Return ("Business", Unlimited, Unlimited, "$299+/yr")
                Case Else
                    Return ("Free", 10, 100, "$0")
            End Select
        End Function

        Private Sub ResetDailyIfNeeded()
            If _lastResetDate < Date.Today Then
                _lastResetDate = Date.Today
                UpdatePlanLimits()
            End If
        End Sub

        Private Sub UpdatePlanLimits()
            Select Case _currentPlan
                Case LicensingPlan.Free
                    _operationsRemaining = 10
                Case LicensingPlan.Personal
                    _operationsRemaining = 100
                Case LicensingPlan.Pro, LicensingPlan.Business
                    _operationsRemaining = Integer.MaxValue
            End Select
        End Sub

        Private Function ValidateLicense(key As String) As Boolean
            ' Basic validation — production would verify against server
            Return Not String.IsNullOrEmpty(key) AndAlso key.Length >= 10
        End Function

        Private Function DeterminePlan(key As String) As LicensingPlan
            ' Simplified plan detection
            If key.StartsWith("BIZ-") Then Return LicensingPlan.Business
            If key.StartsWith("PRO-") Then Return LicensingPlan.Pro
            If key.StartsWith("PER-") Then Return LicensingPlan.Personal
            Return LicensingPlan.Free
        End Function

    End Class

End Namespace