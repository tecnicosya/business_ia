Imports NUnit.Framework
Imports OutlookAIAssistant.Security.Encryption
Imports OutlookAIAssistant.Security.Licensing
Imports OutlookAIAssistant.Security.Privacy
Imports OutlookAIAssistant.Security.Analytics
Imports OutlookAIAssistant.Core.Interfaces
Imports OutlookAIAssistant.Core.Models

Namespace OutlookAIAssistant.Tests

    <TestFixture>
    Public Class SecurityTests

        ' ========================
        ' DPAPI Encryption Tests
        ' ========================

        <Test>
        Public Sub TestEncryptionRoundTrip()
            Dim original = "sk-test-api-key-12345"

            Dim encrypted = DPAPIEncryption.Encrypt(original)
            Dim decrypted = DPAPIEncryption.Decrypt(encrypted)

            Assert.That(encrypted, Is.Not.EqualTo(original))
            Assert.That(decrypted, Is.EqualTo(original))
        End Sub

        <Test>
        Public Sub TestEmptyStringEncryption()
            Dim encrypted = DPAPIEncryption.Encrypt("")
            Assert.That(encrypted, Is.EqualTo(String.Empty))

            Dim decrypted = DPAPIEncryption.Decrypt("")
            Assert.That(decrypted, Is.EqualTo(String.Empty))
        End Sub

        <Test>
        Public Sub TestEncryptionProducesDifferentCiphertexts()
            Dim input = "same-value"

            Dim encrypted1 = DPAPIEncryption.Encrypt(input)
            Dim encrypted2 = DPAPIEncryption.Encrypt(input)

            ' DPAPI with no entropy should still produce different results due to randomness
            Assert.That(encrypted1, Is.Not.EqualTo(encrypted2))
        End Sub

        <Test>
        Public Sub TestDecryptInvalidBase64Throws()
            Assert.That(Sub() DPAPIEncryption.Decrypt("not-base64!"), Throws.InstanceOf(Of InvalidOperationException))
        End Sub

        <Test>
        Public Sub TestStoreAndRetrieveSecureValue()
            Dim testKey = "test_key_" & Guid.NewGuid().ToString("N")
            Dim testValue = "super-secret-api-key"

            DPAPIEncryption.StoreSecureValue(testKey, testValue)
            Dim retrieved = DPAPIEncryption.RetrieveSecureValue(testKey)

            Assert.That(retrieved, Is.EqualTo(testValue))

            ' Cleanup
            DPAPIEncryption.RemoveSecureValue(testKey)
            Assert.That(DPAPIEncryption.SecureValueExists(testKey), Is.False)
        End Sub

        <Test>
        Public Sub TestRetrieveNonExistentKeyReturnsEmpty()
            Dim result = DPAPIEncryption.RetrieveSecureValue("nonexistent_key_" & Guid.NewGuid().ToString("N"))
            Assert.That(result, Is.EqualTo(String.Empty))
        End Sub

        <Test>
        Public Sub TestSecureValueExists()
            Dim testKey = "exists_test_" & Guid.NewGuid().ToString("N")

            Assert.That(DPAPIEncryption.SecureValueExists(testKey), Is.False)

            DPAPIEncryption.StoreSecureValue(testKey, "value")
            Assert.That(DPAPIEncryption.SecureValueExists(testKey), Is.True)

            DPAPIEncryption.RemoveSecureValue(testKey)
            Assert.That(DPAPIEncryption.SecureValueExists(testKey), Is.False)
        End Sub

        <Test>
        Public Sub TestStoreNullValue()
            Dim testKey = "null_test_" & Guid.NewGuid().ToString("N")

            DPAPIEncryption.StoreSecureValue(testKey, Nothing)
            Dim retrieved = DPAPIEncryption.RetrieveSecureValue(testKey)
            Assert.That(retrieved, Is.EqualTo(String.Empty))

            DPAPIEncryption.RemoveSecureValue(testKey)
        End Sub

        ' ========================
        ' License Manager Tests
        ' ========================

        <Test>
        Public Sub TestFreePlanDefaults()
            Dim licenseManager = New LicenseManager()

            Assert.That(licenseManager.GetCurrentPlan(), Is.EqualTo(LicensingPlan.Free))
            Assert.That(licenseManager.CanPerformOperation(), Is.True)
            Assert.That(licenseManager.GetRemainingOperations(), Is.GreaterThan(0))
        End Sub

        <Test>
        Public Sub TestFreePlanDailyLimit()
            Dim licenseManager = New LicenseManager()

            ' Free plan: 10 operations per day
            For i As Integer = 1 To 10
                Assert.That(licenseManager.CanPerformOperation(), Is.True,
                    $"Operation {i} should be allowed")
                licenseManager.RecordOperation()
            Next

            ' 11th should fail
            Assert.That(licenseManager.CanPerformOperation(), Is.False,
                "11th operation on Free plan should be denied")
            Assert.That(licenseManager.GetRemainingOperations(), Is.EqualTo(0))
        End Sub

        <Test>
        Public Sub TestOperationDecrement()
            Dim licenseManager = New LicenseManager()
            Dim before = licenseManager.GetRemainingOperations()

            licenseManager.RecordOperation()

            Dim after = licenseManager.GetRemainingOperations()
            Assert.That(after, Is.EqualTo(before - 1))
        End Sub

        <Test>
        Public Sub TestPlanInfo()
            Dim licenseManager = New LicenseManager()
            Dim (name, dailyLimit, monthlyLimit, price) = licenseManager.GetPlanInfo()

            Assert.That(name, Is.EqualTo("Free"))
            Assert.That(dailyLimit, Is.EqualTo(10))
            Assert.That(monthlyLimit, Is.EqualTo(100))
            Assert.That(price, Is.EqualTo("$0"))
        End Sub

        <Test>
        Public Async Function TestActivatePersonalLicense() As Task
            Dim licenseManager = New LicenseManager()

            Dim result = Await licenseManager.ActivateLicenseAsync("PER-ABCDEFGHIJKL")
            Assert.That(result, Is.True)
            Assert.That(licenseManager.GetCurrentPlan(), Is.EqualTo(LicensingPlan.Personal))
        End Function

        <Test>
        Public Async Function TestActivateProLicense() As Task
            Dim licenseManager = New LicenseManager()

            Dim result = Await licenseManager.ActivateLicenseAsync("PRO-ABCDEFGHIJKL")
            Assert.That(result, Is.True)
            Assert.That(licenseManager.GetCurrentPlan(), Is.EqualTo(LicensingPlan.Pro))
        End Function

        <Test>
        Public Async Function TestActivateBusinessLicense() As Task
            Dim licenseManager = New LicenseManager()

            Dim result = Await licenseManager.ActivateLicenseAsync("BIZ-ABCDEFGHIJKL")
            Assert.That(result, Is.True)
            Assert.That(licenseManager.GetCurrentPlan(), Is.EqualTo(LicensingPlan.Business))
        End Function

        <Test>
        Public Async Function TestInvalidLicenseRejected() As Task
            Dim licenseManager = New LicenseManager()

            Dim result = Await licenseManager.ActivateLicenseAsync("invalid")
            Assert.That(result, Is.False)
            Assert.That(licenseManager.GetCurrentPlan(), Is.EqualTo(LicensingPlan.Free))
        End Function

        <Test>
        Public Async Function TestEmptyLicenseRejected() As Task
            Dim licenseManager = New LicenseManager()

            Dim result = Await licenseManager.ActivateLicenseAsync("")
            Assert.That(result, Is.False)
        End Function

        <Test>
        Public Async Function TestPersonalPlanInfo() As Task
            Dim licenseManager = New LicenseManager()
            Await licenseManager.ActivateLicenseAsync("PER-ABCDEFGHIJKL")

            Dim (name, dailyLimit, monthlyLimit, price) = licenseManager.GetPlanInfo()
            Assert.That(name, Is.EqualTo("Personal"))
            Assert.That(price, Is.EqualTo("$49/yr"))
        End Function

        <Test>
        Public Async Function TestProPlanInfo() As Task
            Dim licenseManager = New LicenseManager()
            Await licenseManager.ActivateLicenseAsync("PRO-ABCDEFGHIJKL")

            Dim (name, dailyLimit, monthlyLimit, price) = licenseManager.GetPlanInfo()
            Assert.That(name, Is.EqualTo("Pro"))
            Assert.That(price, Is.EqualTo("$99/yr"))
        End Function

        <Test>
        Public Async Function TestBusinessPlanInfo() As Task
            Dim licenseManager = New LicenseManager()
            Await licenseManager.ActivateLicenseAsync("BIZ-ABCDEFGHIJKL")

            Dim (name, dailyLimit, monthlyLimit, price) = licenseManager.GetPlanInfo()
            Assert.That(name, Is.EqualTo("Business"))
            Assert.That(price, Is.EqualTo("$299+/yr"))
        End Function

        <Test>
        Public Async Function TestLicenseExpiration() As Task
            Dim licenseManager = New LicenseManager()
            Await licenseManager.ActivateLicenseAsync("PER-ABCDEFGHIJKL")

            Dim expiry = licenseManager.GetExpirationDate()
            Assert.That(expiry.HasValue, Is.True)
            Assert.That(expiry.Value, Is.GreaterThan(Date.Today))
            Assert.That(expiry.Value, Is.LessThanOrEqualTo(Date.Today.AddYears(1).AddDays(1)))
        End Function

        ' ========================
        ' Privacy Manager Tests
        ' ========================

        <Test>
        Public Sub TestDefaultPrivacyMode()
            Dim privacy = New PrivacyManager()

            Assert.That(privacy.CurrentPrivacyMode.Name, Is.EqualTo("Standard"))
            Assert.That(privacy.IsAnonymizePII, Is.False)
            Assert.That(privacy.IsOnPremisesOnly, Is.False)
            Assert.That(privacy.IsAllowAnalytics, Is.True)
        End Sub

        <Test>
        Public Sub TestSetAnonymizedMode()
            Dim privacy = New PrivacyManager()
            privacy.CurrentPrivacyMode = PrivacyMode.Anonymized

            Assert.That(privacy.CurrentPrivacyMode.Name, Is.EqualTo("Anonymized"))
            Assert.That(privacy.IsAnonymizePII, Is.True)
            Assert.That(privacy.IsAllowAnalytics, Is.True)
        End Sub

        <Test>
        Public Sub TestSetOnPremisesMode()
            Dim privacy = New PrivacyManager()
            privacy.CurrentPrivacyMode = PrivacyMode.OnPremisesMode

            Assert.That(privacy.CurrentPrivacyMode.Name, Is.EqualTo("On-Premises"))
            Assert.That(privacy.IsOnPremisesOnly, Is.True)
            Assert.That(privacy.IsLocalOnly, Is.True)
            Assert.That(privacy.IsAllowAnalytics, Is.False)
        End Sub

        <Test>
        Public Sub TestPreviousModeSaved()
            Dim privacy = New PrivacyManager()

            privacy.CurrentPrivacyMode = PrivacyMode.Anonymized
            Assert.That(privacy.PreviousPrivacyMode.Name, Is.EqualTo("Standard"))

            privacy.CurrentPrivacyMode = PrivacyMode.OnPremisesMode
            Assert.That(privacy.PreviousPrivacyMode.Name, Is.EqualTo("Anonymized"))
        End Sub

        <Test>
        Public Sub TestProviderAllowedInStandardMode()
            Dim privacy = New PrivacyManager()

            Assert.That(privacy.IsProviderAllowed("OpenAI", isOnPremises:=False), Is.True)
            Assert.That(privacy.IsProviderAllowed("Ollama", isOnPremises:=True), Is.True)
        End Sub

        <Test>
        Public Sub TestProviderBlockedInOnPremisesMode()
            Dim privacy = New PrivacyManager()
            privacy.CurrentPrivacyMode = PrivacyMode.OnPremisesMode

            Assert.That(privacy.IsProviderAllowed("OpenAI", isOnPremises:=False), Is.False,
                "Cloud provider should be blocked in On-Premises mode")
            Assert.That(privacy.IsProviderAllowed("Ollama", isOnPremises:=True), Is.True,
                "Local provider should be allowed in On-Premises mode")
        End Sub

        <Test>
        Public Sub TestWouldReducePrivacy()
            Dim privacy = New PrivacyManager()

            ' Standard -> Anonymized does not reduce privacy
            Assert.That(privacy.WouldReducePrivacy(PrivacyMode.Anonymized), Is.False)

            privacy.CurrentPrivacyMode = PrivacyMode.OnPremisesMode

            ' On-Premises -> Standard reduces privacy
            Assert.That(privacy.WouldReducePrivacy(PrivacyMode.Standard), Is.True)

            ' On-Premises -> Anonymized reduces privacy
            Assert.That(privacy.WouldReducePrivacy(PrivacyMode.Anonymized), Is.True)
        End Sub

        <Test>
        Public Sub TestPrivacyWarningOnModeDowngrade()
            Dim privacy = New PrivacyManager()
            privacy.CurrentPrivacyMode = PrivacyMode.OnPremisesMode

            Dim warning = privacy.GetPrivacyWarning(PrivacyMode.Standard)
            Assert.That(warning, Is.Not.Empty)
            Assert.That(warning, Does.Contain("Warning"))
            Assert.That(warning, Does.Contain("On-Premises"))
        End Sub

        <Test>
        Public Sub TestNoPrivacyWarningOnUpgrade()
            Dim privacy = New PrivacyManager()
            privacy.CurrentPrivacyMode = PrivacyMode.Standard

            Dim warning = privacy.GetPrivacyWarning(PrivacyMode.OnPremisesMode)
            Assert.That(warning, Is.Empty)
        End Sub

        <Test>
        Public Sub TestGetPrivacyDescription()
            Dim privacy = New PrivacyManager()

            Dim desc = privacy.GetPrivacyDescription()
            Assert.That(desc, Does.Contain("Standard"))
            Assert.That(desc, Is.Not.Empty)

            privacy.CurrentPrivacyMode = PrivacyMode.Anonymized
            desc = privacy.GetPrivacyDescription()
            Assert.That(desc, Does.Contain("Anonymized"))
            Assert.That(desc, Does.Contain("PII"))

            privacy.CurrentPrivacyMode = PrivacyMode.OnPremisesMode
            desc = privacy.GetPrivacyDescription()
            Assert.That(desc, Does.Contain("On-premises"))
        End Sub

        <Test>
        Public Sub TestResetToDefault()
            Dim privacy = New PrivacyManager()
            privacy.CurrentPrivacyMode = PrivacyMode.Anonymized

            privacy.ResetToDefault()
            Assert.That(privacy.CurrentPrivacyMode.Name, Is.EqualTo("Standard"))
        End Sub

        <Test>
        Public Sub TestNullModeThrows()
            Dim privacy = New PrivacyManager()
            Assert.That(Sub() privacy.CurrentPrivacyMode = Nothing,
                Throws.ArgumentNullException)
        End Sub

        ' ========================
        ' Analytics Service Tests
        ' ========================

        <Test>
        Public Sub TestAnalyticsDisabledByDefaultDoesNotThrow()
            ' Should not throw even when analytics are disabled
            Dim analytics = New AnalyticsService(analyticsEnabled:=False)

            analytics.TrackEvent("test_event")
            analytics.TrackEmailProcessed("email1", "summary", 1000)
            analytics.TrackTimeSaved(120.5)

            ' Should still return empty summary
            Dim summary = analytics.GetAnalyticsSummary()
            Assert.That(summary, Is.Not.Null)
            Assert.That(summary.TotalEmailsProcessed, Is.EqualTo(0))
        End Sub

        <Test>
        Public Sub TestAnalyticsSummaryNotNull()
            Dim analytics = New AnalyticsService(analyticsEnabled:=False)
            Dim summary = analytics.GetAnalyticsSummary()

            Assert.That(summary, Is.Not.Null)
            Assert.That(summary.PopularFeatures, Is.Not.Null)
        End Sub

        <Test>
        Public Sub TestFlushDoesNotThrow()
            Dim analytics = New AnalyticsService(analyticsEnabled:=False)
            analytics.Flush()
            Assert.Pass("Flush completed without exception")
        End Sub

        <Test>
        Public Sub TestTrackTokensConsumedDoesNotThrow()
            Dim analytics = New AnalyticsService(analyticsEnabled:=False)
            analytics.TrackTokensConsumed("OpenAI", 1500)
            Assert.Pass("TrackTokensConsumed completed without exception")
        End Sub

        <Test>
        Public Sub TestTrackResponseGeneratedDoesNotThrow()
            Dim analytics = New AnalyticsService(analyticsEnabled:=False)
            analytics.TrackResponseGenerated()
            Assert.Pass("TrackResponseGenerated completed without exception")
        End Sub

        <Test>
        Public Sub TestToggleAnalytics()
            Dim analytics = New AnalyticsService(analyticsEnabled:=False)

            Assert.That(analytics.IsAnalyticsEnabled, Is.False)

            analytics.IsAnalyticsEnabled = True
            Assert.That(analytics.IsAnalyticsEnabled, Is.True)
        End Sub

    End Class

End Namespace