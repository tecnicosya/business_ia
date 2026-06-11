Namespace OutlookAIAssistant.OutlookModule.TaskPane

    ''' <summary>
    ''' Manages custom task pane registration and lifecycle.
    ''' Task panes are docked UI panels within the Outlook inspector window.
    ''' </summary>
    Public Class TaskPaneManager
        Implements IDisposable

        Private _isDisposed As Boolean = False

        Public Sub New()
        End Sub

        ''' <summary>
        ''' Registers a custom task pane for the AI sidebar.
        ''' </summary>
        Public Sub RegisterTaskPane()
            ' In production, this uses Microsoft.Office.Tools.CustomTaskPane
            ' to add the WPF sidebar as a docked panel in the inspector
            System.Diagnostics.Debug.WriteLine("Task pane registered.")
        End Sub

        ''' <summary>
        ''' Removes the custom task pane.
        ''' </summary>
        Public Sub RemoveTaskPane()
            System.Diagnostics.Debug.WriteLine("Task pane removed.")
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If Not _isDisposed Then
                RemoveTaskPane()
                _isDisposed = True
            End If
        End Sub

    End Class

End Namespace