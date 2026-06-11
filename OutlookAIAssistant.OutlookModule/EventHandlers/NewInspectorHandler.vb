Imports OutlookAIAssistant.Core.Models

Namespace OutlookAIAssistant.OutlookModule.EventHandlers

    ''' <summary>
    ''' Handles the NewInspector event — triggered when an email is opened in a new window.
    ''' Shows the AI sidebar for the opened email.
    ''' </summary>
    Public Class NewInspectorHandler

        Private ReadOnly _outlookApp As Microsoft.Office.Interop.Outlook.Application
        Private ReadOnly _aiEngine As AIEngine.AIEngine

        Public Sub New(outlookApp As Microsoft.Office.Interop.Outlook.Application,
                       aiEngine As AIEngine.AIEngine)
            _outlookApp = outlookApp
            _aiEngine = aiEngine
        End Sub

        ''' <summary>
        ''' Called when a new inspector (email window) is opened.
        ''' </summary>
        Public Sub OnNewInspector(inspector As Microsoft.Office.Interop.Outlook.Inspector)
            Try
                Dim mailItem = TryCast(inspector.CurrentItem, Microsoft.Office.Interop.Outlook.MailItem)
                If mailItem IsNot Nothing Then
                    System.Diagnostics.Debug.WriteLine($"NewInspectorHandler: Opening email '{mailItem.Subject}'")

                    ' Load the email into the sidebar for quick actions
                    Dim emailMessage = New EmailMessage With {
                        .Id = mailItem.EntryID,
                        .Subject = mailItem.Subject,
                        .Body = mailItem.Body,
                        .SenderEmail = mailItem.SenderEmailAddress,
                        .SenderName = mailItem.SenderName,
                        .ReceivedTime = mailItem.ReceivedTime
                    }

                    ' Notify sidebar of the opened email
                    Dim addIn = Globals.ThisAddIn
                    addIn.GetSidebarManager().LoadEmail(emailMessage)
                End If
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine($"NewInspectorHandler.OnNewInspector error: {ex.Message}")
            End Try
        End Sub

    End Class

End Namespace