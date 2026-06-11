Imports NUnit.Framework
Imports OutlookAIAssistant.Rules

Namespace OutlookAIAssistant.Tests

    <TestFixture>
    Public Class RulesEngineTests

        <Test>
        Public Sub TestRuleCreation()
            Dim rule = New Rule With {
                .Id = "rule-001",
                .Name = "Auto-categorize invoices",
                .IsEnabled = True,
                .Priority = 10
            }

            Assert.That(rule.Id, Is.EqualTo("rule-001"))
            Assert.That(rule.IsEnabled, Is.True)
            Assert.That(rule.Priority, Is.EqualTo(10))
        End Sub

        <Test>
        Public Sub TestTemplateEngine()
            Dim engine = New TemplateEngine()
            Dim parameters = New Dictionary(Of String, String) From {
                {"name", "John"},
                {"subject", "Support Request"}
            }

            Dim result = engine.Render("Hello {{name}}, regarding {{subject}}...", parameters)
            Assert.That(result, Is.EqualTo("Hello John, regarding Support Request..."))
        End Sub

        <Test>
        Public Sub TestRulePriorityOrder()
            Dim highPriority = New Rule With {.Id = "high", .Priority = 1}
            Dim lowPriority = New Rule With {.Id = "low", .Priority = 100}

            Assert.That(highPriority.Priority, Is.LessThan(lowPriority.Priority))
        End Sub

    End Class

End Namespace