Imports OutlookAIAssistant.Core.Models

Namespace OutlookAIAssistant.OutlookModule.EventHandlers

    ''' <summary>
    ''' Handles the ItemSend event — triggered before an email is sent.
    ''' Can intercept and modify the outgoing email (proofread, check, categorize).
    ''' </summary>
    Public Class ItemSendHandler

        Private ReadOnly _outlookApp As Microsoft.Office.Interop.Outlook.Application
        Private ReadOnly _aiEngine As AIEngine.AIEngine

        Public Sub New(outlookApp As Microsoft.Office.Interop.Outlook.Application,
                       aiEngine As AIEngine.AIEngine)
            _outlookApp = outlookApp
            _aiEngine = aiEngine
        End Sub

        ''' <summary>
        ''' Called before an email is sent. Return False to cancel sending.
        ''' </summary>
        Public Function OnItemSend(item As Object, ByRef cancel As Boolean) As Boolean
            Try
                Dim mailItem = TryCast(item, Microsoft.Office.Interop.Outlook.MailItem)
                If mailItem IsNot Nothing Then
                    ' Optional: proofread before sending
                    ' Optional: check for missing attachments
                    ' Optional: classify and move to appropriate folder
                    System.Diagnostics.Debug.WriteLine($"ItemSendHandler: Email '{mailItem.Subject}' is being sent.")
                End If
                Return True
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine($"ItemSendHandler.OnItemSend error: {ex.Message}")
                Return True
            End Try
        End Function

    End Class

End Namespace