Namespace OutlookAIAssistant.OutlookModule.EventHandlers

    ''' <summary>
    ''' Handles item change events — folder changes, item property changes.
    ''' </summary>
    Public Class ItemChangeHandler

        Private ReadOnly _outlookApp As Microsoft.Office.Interop.Outlook.Application

        Public Sub New(outlookApp As Microsoft.Office.Interop.Outlook.Application)
            _outlookApp = outlookApp
        End Sub

        ''' <summary>
        ''' Called when an advanced search completes.
        ''' </summary>
        Public Sub OnAdvancedSearchComplete(searchObject As Microsoft.Office.Interop.Outlook.Search)
            Try
                System.Diagnostics.Debug.WriteLine($"Advanced search '{searchObject.Name}' completed.")
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine($"ItemChangeHandler.OnAdvancedSearchComplete error: {ex.Message}")
            End Try
        End Sub

        ''' <summary>
        ''' Called when a folder's item properties change.
        ''' </summary>
        Public Sub OnItemChange(item As Object)
            Try
                Dim mailItem = TryCast(item, Microsoft.Office.Interop.Outlook.MailItem)
                If mailItem IsNot Nothing Then
                    System.Diagnostics.Debug.WriteLine($"ItemChangeHandler: Item '{mailItem.Subject}' changed.")
                End If
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine($"ItemChangeHandler.OnItemChange error: {ex.Message}")
            End Try
        End Sub

    End Class

End Namespace