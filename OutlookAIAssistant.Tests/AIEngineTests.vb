Imports NUnit.Framework
Imports OutlookAIAssistant.Core.Models
Imports OutlookAIAssistant.Core.Enums

Namespace OutlookAIAssistant.Tests

    <TestFixture>
    Public Class AIEngineTests

        <Test>
        Public Sub TestAIModelCreation()
            Dim request = New AIRequest With {
                .RequestId = "test-001",
                .Content = "Test email content for AI processing.",
                .Subject = "Test Subject",
                .ProviderName = "OpenAI"
            }

            Assert.That(request.RequestId, Is.EqualTo("test-001"))
            Assert.That(request.Content, Is.Not.Empty)
            Assert.That(request.CreatedAt, Is.LessThanOrEqualTo(DateTime.UtcNow))
        End Sub

        <Test>
        Public Sub TestAIResponseCreation()
            Dim response = New AIResponse With {
                .RequestId = "test-001",
                .Content = "This is a generated response.",
                .ProviderName = "OpenAI",
                .ModelUsed = "gpt-4o",
                .IsSuccess = True,
                .TotalTokens = 150
            }

            Assert.That(response.IsSuccess, Is.True)
            Assert.That(response.Content, Is.EqualTo("This is a generated response."))
            Assert.That(response.TotalTokens, Is.EqualTo(150))
        End Sub

        <Test>
        Public Sub TestClassificationTypeEnum()
            Dim values = [Enum].GetValues(GetType(ClassificationType))
            Assert.That(values.Length, Is.GreaterThan(0))
            Assert.That(ClassificationType.Work, Is.EqualTo(CType(1, ClassificationType)))
            Assert.That(ClassificationType.Unknown, Is.EqualTo(CType(0, ClassificationType)))
        End Sub

        <Test>
        Public Sub TestEmailMessageProperties()
            Dim email = New EmailMessage With {
                .Id = "msg-001",
                .Subject = "Test Email",
                .Body = "This is a test email body.",
                .SenderEmail = "test@example.com",
                .IsRead = False,
                .HasAttachments = True
            }

            Assert.That(email.Subject, Is.EqualTo("Test Email"))
            Assert.That(email.SenderEmail, Is.EqualTo("test@example.com"))
            Assert.That(email.IsRead, Is.False)
            Assert.That(email.HasAttachments, Is.True)
        End Sub

        <Test>
        Public Sub TestProviderConfigDefaults()
            Dim config = New ProviderConfig With {
                .Name = "Test Provider",
                .ProviderType = "OpenAI",
                .ApiKey = "sk-test-key"
            }

            Assert.That(config.MaxTokens, Is.EqualTo(2048))
            Assert.That(config.Temperature, Is.EqualTo(0.7))
            Assert.That(config.TimeoutSeconds, Is.EqualTo(60))
            Assert.That(config.IsEnabled, Is.True)
            Assert.That(config.IsOnPremises, Is.False)
        End Sub

        <Test>
        Public Sub TestTrustScoreThreshold()
            Dim trusted = New TrustScore With {.Score = 0.85, .InteractionCount = 20}
            Dim untrusted = New TrustScore With {.Score = 0.3, .InteractionCount = 1}

            Assert.That(trusted.IsTrusted(), Is.True)
            Assert.That(untrusted.IsTrusted(), Is.False)
        End Sub

        <Test>
        Public Sub TestWhitelistedOverridesScore()
            Dim whitelisted = New TrustScore With {
                .Score = 0.2,
                .IsWhitelisted = True
            }
            Assert.That(whitelisted.IsTrusted(), Is.True)
        End Sub

        <Test>
        Public Sub TestBlacklistedOverridesScore()
            Dim blacklisted = New TrustScore With {
                .Score = 0.9,
                .IsBlacklisted = True
            }
            Assert.That(blacklisted.IsTrusted(), Is.False)
        End Sub

    End Class

End Namespace