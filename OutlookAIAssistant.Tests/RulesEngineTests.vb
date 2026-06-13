Imports NUnit.Framework
Imports OutlookAIAssistant.Rules
Imports OutlookAIAssistant.Core.Models
Imports OutlookAIAssistant.Core.Enums

Namespace OutlookAIAssistant.Tests

    <TestFixture>
    Public Class RulesEngineTests

        Private _engine As RulesEngine

        <SetUp>
        Public Sub Setup()
            _engine = New RulesEngine()
        End Sub

        <TearDown>
        Public Sub TearDown()
            _engine.ClearRules()
        End Sub

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
        Public Sub TestAddAndGetRules()
            Dim rule1 = New Rule With {.Id = "r1", .Name = "Rule 1", .Priority = 10}
            Dim rule2 = New Rule With {.Id = "r2", .Name = "Rule 2", .Priority = 20}

            _engine.AddRule(rule1)
            _engine.AddRule(rule2)

            Dim rules = _engine.GetRules()
            Assert.That(rules.Count, Is.EqualTo(2))
            Assert.That(rules(0).Id, Is.EqualTo("r1"))
            Assert.That(rules(1).Id, Is.EqualTo("r2"))
        End Sub

        <Test>
        Public Sub TestRemoveRule()
            Dim rule = New Rule With {.Id = "r1", .Name = "Rule 1"}
            _engine.AddRule(rule)
            Assert.That(_engine.GetRuleCount(), Is.EqualTo(1))

            _engine.RemoveRule("r1")
            Assert.That(_engine.GetRuleCount(), Is.EqualTo(0))
        End Sub

        <Test>
        Public Sub TestRulePriorityOrder()
            Dim highPriority = New Rule With {.Id = "high", .Name = "High", .Priority = 1}
            Dim lowPriority = New Rule With {.Id = "low", .Name = "Low", .Priority = 100}

            _engine.AddRule(highPriority)
            _engine.AddRule(lowPriority)

            Dim rules = _engine.GetRules()
            Assert.That(rules(0).Id, Is.EqualTo("high"))
            Assert.That(rules(1).Id, Is.EqualTo("low"))
        End Sub

        <Test>
        Public Sub TestDisabledRuleIsSkipped()
            Dim rule = New Rule With {
                .Id = "disabled",
                .Name = "Disabled Rule",
                .IsEnabled = False,
                .Conditions = New List(Of RuleCondition)()
            }

            _engine.AddRule(rule)
            Assert.That(_engine.GetRuleCount(), Is.EqualTo(1))

            ' Evaluate against any email — disabled rule should not trigger
            Dim email = CreateTestEmail()
            Dim task = _engine.EvaluateAsync(email)
            task.Wait()
            Dim actions = task.Result
            Assert.That(actions.Count, Is.EqualTo(0))
        End Sub

        <Test>
        Public Sub TestConditionEquals()
            Dim rule = New Rule With {
                .Id = "r1",
                .Name = "Match sender",
                .Priority = 1,
                .Conditions = New List(Of RuleCondition) From {
                    New RuleCondition With {.Field = "sender", .Operator = "equals", .Value = "test@example.com"}
                },
                .Actions = New List(Of RuleAction) From {
                    New RuleAction With {.Type = "flag", .Value = "Follow up"}
                }
            }
            _engine.AddRule(rule)

            Dim email = CreateTestEmail(sender:="test@example.com")
            Dim task = _engine.EvaluateAsync(email)
            task.Wait()
            Dim actions = task.Result
            Assert.That(actions.Count, Is.EqualTo(1))
            Assert.That(actions(0).Type, Is.EqualTo("flag"))
        End Sub

        <Test>
        Public Sub TestConditionNotEquals()
            Dim rule = New Rule With {
                .Id = "r1",
                .Name = "Not match different sender",
                .Priority = 1,
                .Conditions = New List(Of RuleCondition) From {
                    New RuleCondition With {.Field = "sender", .Operator = "notequals", .Value = "other@example.com"}
                },
                .Actions = New List(Of RuleAction) From {
                    New RuleAction With {.Type = "categorize", .Value = "Important"}
                }
            }
            _engine.AddRule(rule)

            Dim email = CreateTestEmail(sender:="test@example.com")
            Dim task = _engine.EvaluateAsync(email)
            task.Wait()
            Dim actions = task.Result
            Assert.That(actions.Count, Is.EqualTo(1))
        End Sub

        <Test>
        Public Sub TestConditionContains()
            Dim rule = New Rule With {
                .Id = "r1",
                .Name = "Invoice subject",
                .Priority = 1,
                .Conditions = New List(Of RuleCondition) From {
                    New RuleCondition With {.Field = "subject", .Operator = "contains", .Value = "invoice"}
                },
                .Actions = New List(Of RuleAction) From {
                    New RuleAction With {.Type = "categorize", .Value = "Invoices"}
                }
            }
            _engine.AddRule(rule)

            Dim email = CreateTestEmail(subject:="Invoice #12345 from Vendor")
            Dim task = _engine.EvaluateAsync(email)
            task.Wait()
            Dim actions = task.Result
            Assert.That(actions.Count, Is.EqualTo(1))
            Assert.That(actions(0).Value, Is.EqualTo("Invoices"))
        End Sub

        <Test>
        Public Sub TestAndConditionAllMustMatch()
            Dim rule = New Rule With {
                .Id = "r1",
                .Name = "Invoice from specific sender",
                .Priority = 1,
                .ConditionGroupOperator = ConditionGroupOperator.And,
                .Conditions = New List(Of RuleCondition) From {
                    New RuleCondition With {.Field = "subject", .Operator = "contains", .Value = "invoice"},
                    New RuleCondition With {.Field = "sender", .Operator = "contains", .Value = "vendor.com"}
                },
                .Actions = New List(Of RuleAction) From {
                    New RuleAction With {.Type = "movetofolder", .Value = "Invoices"}
                }
            }
            _engine.AddRule(rule)

            ' Both conditions match
            Dim email1 = CreateTestEmail(subject:="Invoice from Vendor", sender:="billing@vendor.com")
            Dim task1 = _engine.EvaluateAsync(email1)
            task1.Wait()
            Assert.That(task1.Result.Count, Is.EqualTo(1))

            ' Only one condition matches
            Dim email2 = CreateTestEmail(subject:="Invoice from Vendor", sender:="other@example.com")
            Dim task2 = _engine.EvaluateAsync(email2)
            task2.Wait()
            Assert.That(task2.Result.Count, Is.EqualTo(0))
        End Sub

        <Test>
        Public Sub TestOrConditionAnyCanMatch()
            Dim rule = New Rule With {
                .Id = "r1",
                .Name = "Urgent or high priority",
                .Priority = 1,
                .ConditionGroupOperator = ConditionGroupOperator.Or,
                .Conditions = New List(Of RuleCondition) From {
                    New RuleCondition With {.Field = "subject", .Operator = "contains", .Value = "urgent"},
                    New RuleCondition With {.Field = "subject", .Operator = "contains", .Value = "important"}
                },
                .Actions = New List(Of RuleAction) From {
                    New RuleAction With {.Type = "flag", .Value = "Urgent"}
                }
            }
            _engine.AddRule(rule)

            ' First condition matches
            Dim email1 = CreateTestEmail(subject:="URGENT: Please review")
            Dim task1 = _engine.EvaluateAsync(email1)
            task1.Wait()
            Assert.That(task1.Result.Count, Is.EqualTo(1))

            ' Second condition matches
            Dim email2 = CreateTestEmail(subject:="Important announcement")
            Dim task2 = _engine.EvaluateAsync(email2)
            task2.Wait()
            Assert.That(task2.Result.Count, Is.EqualTo(1))

            ' Neither matches
            Dim email3 = CreateTestEmail(subject:="Weekly newsletter")
            Dim task3 = _engine.EvaluateAsync(email3)
            task3.Wait()
            Assert.That(task3.Result.Count, Is.EqualTo(0))
        End Sub

        <Test>
        Public Sub TestMultipleActionsFromRule()
            Dim rule = New Rule With {
                .Id = "r1",
                .Name = "Process invoice",
                .Priority = 1,
                .Conditions = New List(Of RuleCondition) From {
                    New RuleCondition With {.Field = "subject", .Operator = "contains", .Value = "invoice"}
                },
                .Actions = New List(Of RuleAction) From {
                    New RuleAction With {.Type = "categorize", .Value = "Invoices"},
                    New RuleAction With {.Type = "movetofolder", .Value = "Invoices"},
                    New RuleAction With {.Type = "flag", .Value = "Review"}
                }
            }
            _engine.AddRule(rule)

            Dim email = CreateTestEmail(subject:="Invoice #999")
            Dim task = _engine.EvaluateAsync(email)
            task.Wait()
            Assert.That(task.Result.Count, Is.EqualTo(3))
        End Sub

        <Test>
        Public Sub TestRunPipelineAsync()
            Dim rule = New Rule With {
                .Id = "r1",
                .Name = "Flag invoices",
                .Priority = 1,
                .Conditions = New List(Of RuleCondition) From {
                    New RuleCondition With {.Field = "subject", .Operator = "contains", .Value = "invoice"}
                },
                .Actions = New List(Of RuleAction) From {
                    New RuleAction With {.Type = "flag", .Value = "Review"}
                }
            }
            _engine.AddRule(rule)

            Dim email = CreateTestEmail(subject:="Invoice from Vendor")
            Dim task = _engine.RunPipelineAsync(email)
            task.Wait()
            Dim result = task.Result

            Assert.That(result.Processed, Is.True)
            Assert.That(result.Actions.Count, Is.EqualTo(1))
            Assert.That(result.Errors.Count, Is.EqualTo(0))
            Assert.That(result.Summary, Does.Contain("triggered"))
        End Sub

        <Test>
        Public Sub TestRuleValidation_EmptyId()
            Dim rule = New Rule With {
                .Id = "",
                .Name = "Bad Rule"
            }

            Dim errors = RuleHelper.ValidateRule(rule)
            Assert.That(errors.Count, Is.GreaterThan(0))
            Assert.That(errors(0), Does.Contain("Id"))
        End Sub

        <Test>
        Public Sub TestRuleValidation_EmptyName()
            Dim rule = New Rule With {
                .Id = "r1",
                .Name = ""
            }

            Dim errors = RuleHelper.ValidateRule(rule)
            Assert.That(errors.Count, Is.GreaterThan(0))
            Assert.That(errors(0), Does.Contain("Name"))
        End Sub

        <Test>
        Public Sub TestAddRule_InvalidThrows()
            Dim rule = New Rule With {
                .Id = "",
                .Name = ""
            }

            Assert.That(Sub() _engine.AddRule(rule), Throws.ArgumentException)
        End Sub

        <Test>
        Public Sub TestSerializeDeserializeRule()
            Dim rule = New Rule With {
                .Id = "r1",
                .Name = "Test Rule",
                .Priority = 5,
                .IsEnabled = True,
                .Conditions = New List(Of RuleCondition) From {
                    New RuleCondition With {.Field = "subject", .Operator = "contains", .Value = "test"}
                },
                .Actions = New List(Of RuleAction) From {
                    New RuleAction With {.Type = "flag", .Value = "Follow up"}
                }
            }

            Dim json = RuleHelper.SerializeRule(rule)
            Assert.That(json, Is.Not.Empty)
            Assert.That(json, Does.Contain("Test Rule"))

            Dim deserialized = RuleHelper.DeserializeRule(json)
            Assert.That(deserialized, Is.Not.Nothing)
            Assert.That(deserialized.Id, Is.EqualTo("r1"))
            Assert.That(deserialized.Name, Is.EqualTo("Test Rule"))
            Assert.That(deserialized.Conditions.Count, Is.EqualTo(1))
        End Sub

        <Test>
        Public Sub TestCloneRule()
            Dim original = New Rule With {
                .Id = "r1",
                .Name = "Original",
                .Priority = 10,
                .Conditions = New List(Of RuleCondition) From {
                    New RuleCondition With {.Field = "subject", .Operator = "contains", .Value = "test"}
                }
            }

            Dim clone = RuleHelper.CloneRule(original)
            Assert.That(clone.Id, Is.EqualTo(original.Id))
            Assert.That(clone.Name, Is.EqualTo(original.Name))
            Assert.That(clone.Conditions.Count, Is.EqualTo(original.Conditions.Count))

            ' Verify it's a deep copy
            clone.Name = "Modified"
            Assert.That(original.Name, Is.EqualTo("Original"))
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
        Public Sub TestTemplateEngine_UnknownVariableRemoved()
            Dim engine = New TemplateEngine()
            Dim parameters = New Dictionary(Of String, String) From {
                {"name", "Alice"}
            }

            Dim result = engine.Render("Hello {{name}}, {{subject}} was received.", parameters)
            Assert.That(result, Is.EqualTo("Hello Alice,  was received."))
        End Sub

        <Test>
        Public Sub TestTemplateEngine_DefaultVariables()
            Dim engine = New TemplateEngine()
            Dim parameters = New Dictionary(Of String, String) From {
                {"name", "Bob"}
            }

            Dim result = engine.Render("On {{date}}, {{name}} said...", parameters)
            Assert.That(result, Does.Contain("Bob"))
            Assert.That(result, Does.Match("\d{4}-\d{2}-\d{2}"))
        End Sub

        <Test>
        Public Sub TestTemplateEngine_ConditionalIf()
            Dim engine = New TemplateEngine()
            Dim parameters = New Dictionary(Of String, String) From {
                {"urgent", "true"},
                {"subject", "Urgent issue"}
            }

            Dim result = engine.Render(
                "{{#if urgent}}URGENT: {{subject}}{{/if}}",
                parameters)

            Assert.That(result, Is.EqualTo("URGENT: Urgent issue"))
        End Sub

        <Test>
        Public Sub TestTemplateEngine_ConditionalIfFalse()
            Dim engine = New TemplateEngine()
            Dim parameters = New Dictionary(Of String, String) From {
                {"urgent", "false"},
                {"subject", "Normal email"}
            }

            Dim result = engine.Render(
                "{{#if urgent}}URGENT: {{subject}}{{/if}}Normal message",
                parameters)

            Assert.That(result, Is.EqualTo("Normal message"))
        End Sub

        <Test>
        Public Sub TestTemplateEngine_ConditionalUnless()
            Dim engine = New TemplateEngine()
            Dim parameters = New Dictionary(Of String, String) From {
                {"is_spam", "true"}
            }

            Dim result = engine.Render(
                "{{#unless is_spam}}This is not spam{{/unless}}",
                parameters)

            Assert.That(result, Is.EqualTo(""))
        End Sub

        <Test>
        Public Sub TestTemplateEngine_Eq()
            Dim engine = New TemplateEngine()
            Dim parameters = New Dictionary(Of String, String) From {
                {"classification", "support"}
            }

            Dim result = engine.Render(
                "{{#eq classification support}}This is a support request{{/eq}}",
                parameters)

            Assert.That(result, Is.EqualTo("This is a support request"))
        End Sub

        <Test>
        Public Sub TestTemplateEngine_NamedTemplate()
            Dim engine = New TemplateEngine()
            Dim parameters = New Dictionary(Of String, String) From {
                {"subject", "Test Subject"}
            }

            Dim result = engine.RenderNamed("support_acknowledgment", parameters)
            Assert.That(result, Does.Contain("Test Subject"))
            Assert.That(result, Does.Contain("24 hours"))
        End Sub

        <Test>
        Public Sub TestTemplateEngine_NamedTemplateNotFound()
            Dim engine = New TemplateEngine()
            Assert.That(
                Sub() engine.RenderNamed("nonexistent", New Dictionary(Of String, String)()),
                Throws.TypeOf(Of KeyNotFoundException)())
        End Sub

        <Test>
        Public Sub TestSupportTemplateAcknowledgment()
            Dim result = Templates.SupportTemplate.Acknowledgment("Alice", "Help with login")
            Assert.That(result, Does.Contain("Alice"))
            Assert.That(result, Does.Contain("Help with login"))
            Assert.That(result, Does.Contain("24 hours"))
        End Sub

        <Test>
        Public Sub TestSupportTemplateResolved()
            Dim result = Templates.SupportTemplate.Resolved("Bob", "Password reset")
            Assert.That(result, Does.Contain("Bob"))
            Assert.That(result, Does.Contain("Password reset"))
            Assert.That(result, Does.Contain("resolved"))
        End Sub

        <Test>
        Public Sub TestSalesTemplateFollowUp()
            Dim result = Templates.SalesTemplate.FollowUp("Client", "New proposal", "Sales Rep")
            Assert.That(result, Does.Contain("Client"))
            Assert.That(result, Does.Contain("New proposal"))
            Assert.That(result, Does.Contain("Sales Rep"))
        End Sub

        <Test>
        Public Sub TestGeneralAcknowledgmentTemplate()
            Dim result = Templates.GeneralAcknowledgmentTemplate.GeneralReply("User", "Question", "Support Agent")
            Assert.That(result, Does.Contain("User"))
            Assert.That(result, Does.Contain("Question"))
            Assert.That(result, Does.Contain("Support Agent"))
        End Sub

        <Test>
        Public Sub TestRuleHelper_GetActionDisplayName()
            Assert.That(RuleHelper.GetActionDisplayName("createdraft"), Is.EqualTo("Create Draft Reply"))
            Assert.That(RuleHelper.GetActionDisplayName("moveToFolder"), Is.EqualTo("Move to Folder"))
            Assert.That(RuleHelper.GetActionDisplayName("flag"), Is.EqualTo("Flag for Follow-Up"))
        End Sub

        <Test>
        Public Sub TestRuleHelper_GetFieldDisplayName()
            Assert.That(RuleHelper.GetFieldDisplayName("classification"), Is.EqualTo("Classification"))
            Assert.That(RuleHelper.GetFieldDisplayName("senderdomain"), Is.EqualTo("Sender Domain"))
        End Sub

        <Test>
        Public Sub TestPipelineResultDefaults()
            Dim result = New PipelineResult()
            Assert.That(result.Processed, Is.False)
            Assert.That(result.Actions, Is.Not.Nothing)
            Assert.That(result.ExecutedActions, Is.Not.Nothing)
            Assert.That(result.Errors, Is.Not.Nothing)
            Assert.That(result.WasAutoSent, Is.False)
        End Sub

        <Test>
        Public Sub TestRulePriorityOverrides()
            ' Higher priority (lower number) rules should evaluate first
            Dim highPriority = New Rule With {
                .Id = "alert",
                .Name = "Alert rule",
                .Priority = 1,
                .Conditions = New List(Of RuleCondition) From {
                    New RuleCondition With {.Field = "subject", .Operator = "contains", .Value = "alert"}
                },
                .Actions = New List(Of RuleAction) From {
                    New RuleAction With {.Type = "flag", .Value = "Alert"}
                }
            }

            Dim lowPriority = New Rule With {
                .Id = "catchall",
                .Name = "Catch-all",
                .Priority = 100,
                .Conditions = New List(Of RuleCondition)(),
                .Actions = New List(Of RuleAction) From {
                    New RuleAction With {.Type = "categorize", .Value = "General"}
                }
            }

            _engine.AddRule(lowPriority)
            _engine.AddRule(highPriority)

            ' When subject contains "alert", both rules should match
            Dim email = CreateTestEmail(subject:="System alert: server down")
            Dim task = _engine.EvaluateAsync(email)
            task.Wait()
            Dim actions = task.Result
            Assert.That(actions.Count, Is.EqualTo(2))
        End Sub

        Private Function CreateTestEmail(Optional subject As String = "Test Subject",
                                          Optional body As String = "Test body content.",
                                          Optional sender As String = "sender@example.com",
                                          Optional senderName As String = "Test Sender") As EmailMessage
            Return New EmailMessage With {
                .Id = Guid.NewGuid().ToString(),
                .Subject = subject,
                .Body = body,
                .SenderEmail = sender,
                .SenderName = senderName,
                .IsRead = False,
                .HasAttachments = False,
                .ReceivedTime = DateTime.UtcNow
            }
        End Function

    End Class

End Namespace