Imports OutlookAIAssistant.Core.Models

Namespace OutlookAIAssistant.OutlookModule.EventHandlers

    ''' <summary>
    ''' Handles the NewMailEx event — triggered when a new email arrives in the Inbox.
    ''' </summary>
    Public Class ItemAddHandler

        Private ReadOnly _outlookApp As Microsoft.Office.Interop.Outlook.Application
        Private ReadOnly _aiEngine As AIEngine.AIEngine
        Private ReadOnly _rulesEngine As Rules.RulesEngine

        Public Sub New(outlookApp As Microsoft.Office.Interop.Outlook.Application,
                       aiEngine As AIEngine.AIEngine,
                       rulesEngine As Rules.RulesEngine)
            _outlookApp = outlookApp
            _aiEngine = aiEngine
            _rulesEngine = rulesEngine
        End Sub

        ''' <summary>
        ''' Called when a new email arrives (NewMailEx event).
        ''' EntryID of the new item is passed as parameter.
        ''' </summary>
        Public Sub OnNewMailEx(entryIdCollection As String)
            Try
                ' There can be multiple EntryIDs separated by commas
                Dim entryIds = entryIdCollection.Split(","c)
                For Each entryId In entryIds
                    entryId = entryId.Trim()
                    If String.IsNullOrEmpty(entryId) Then Continue For

                    ' Get the mail item from the EntryID
                    Dim mailItem = TryCast(_outlookApp.Session.GetItemFromID(entryId),
                        Microsoft.Office.Interop.Outlook.MailItem)

                    If mailItem IsNot Nothing Then
                        ProcessNewEmail(mailItem)
                    End If
                Next
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine($"ItemAddHandler.OnNewMailEx error: {ex.Message}")
            End Try
        End Sub

        ''' <summary>
        ''' Processes a newly received email through AI and rules engines.
        ''' </summary>
        Private Async Sub ProcessNewEmail(mailItem As Microsoft.Office.Interop.Outlook.MailItem)
            Try
                Dim emailMessage = New EmailMessage With {
                    .Id = mailItem.EntryID,
                    .Subject = mailItem.Subject,
                    .Body = mailItem.Body,
                    .SenderEmail = mailItem.SenderEmailAddress,
                    .SenderName = mailItem.SenderName,
                    .ToRecipients = GetRecipients(mailItem.To),
                    .ReceivedTime = mailItem.ReceivedTime,
                    .Size = mailItem.Size,
                    .ConversationId = mailItem.ConversationID,
                    .HasAttachments = mailItem.Attachments.Count > 0,
                    .IsRead = mailItem.UnRead = False
                }

                ' Classify the email
                Dim classification = Await _aiEngine.ClassifyAsync(
                    New AIRequest With {
                        .Content = mailItem.Body,
                        .Subject = mailItem.Subject,
                        .ProviderName = _aiEngine.GetConfiguredProviders().FirstOrDefault()
                    }
                )

                If classification IsNot Nothing Then
                    emailMessage.Classification = classification
                    emailMessage.IsAICategorized = True
                End If

                ' Run rules engine
                Dim actions = Await _rulesEngine.EvaluateAsync(emailMessage)
                For Each action In actions
                    ' Execute each rule action
                    System.Diagnostics.Debug.WriteLine($"Rule action: {action.Type} for email: {emailMessage.Subject}")
                Next

            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine($"ProcessNewEmail error: {ex.Message}")
            End Try
        End Sub

        Private Function GetRecipients(recipients As String) As String
            Return If(recipients, "")
        End Function

    End Class

End Namespace