Imports NUnit.Framework
Imports OutlookAIAssistant.Security.Encryption
Imports OutlookAIAssistant.Security.Licensing
Imports OutlookAIAssistant.Core.Interfaces

Namespace OutlookAIAssistant.Tests

    <TestFixture>
    Public Class SecurityTests

        <Test>
        Public Sub TestEncryptionRoundTrip()
            Dim encryption = New DPAPIEncryption()
            Dim original = "sk-test-api-key-12345"

            Dim encrypted = encryption.Encrypt(original)
            Dim decrypted = encryption.Decrypt(encrypted)

            Assert.That(encrypted, Is.Not.EqualTo(original))
            Assert.That(decrypted, Is.EqualTo(original))
        End Sub

        <Test>
        Public Sub TestEmptyStringEncryption()
            Dim encryption = New DPAPIEncryption()

            Dim encrypted = encryption.Encrypt("")
            Assert.That(encrypted, Is.EqualTo(String.Empty))

            Dim decrypted = encryption.Decrypt("")
            Assert.That(decrypted, Is.EqualTo(String.Empty))
        End Sub

        <Test>
        Public Sub TestFreePlanLimits()
            Dim licenseManager = New LicenseManager()
            Assert.That(licenseManager.GetCurrentPlan(), Is.EqualTo(LicensingPlan.Free))
            Assert.That(licenseManager.CanPerformOperation(), Is.True)
            Assert.That(licenseManager.GetRemainingOperations(), Is.GreaterThan(0))
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
        End Sub

    End Class

End Namespace