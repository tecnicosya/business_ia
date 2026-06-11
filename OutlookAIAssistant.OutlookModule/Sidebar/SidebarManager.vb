Imports OutlookAIAssistant.Core.Models

Namespace OutlookAIAssistant.OutlookModule.Sidebar

    ''' <summary>
    ''' Manages the AI sidebar visibility and lifecycle.
    ''' Coordinates between the WPF sidebar control and the Outlook inspector.
    ''' </summary>
    Public Class SidebarManager
        Implements IDisposable

        Private ReadOnly _aiEngine As AIEngine.AIEngine
        Private _sidebarControl As AISidebarWpf
        Private _viewModel As SidebarViewModel
        Private _isVisible As Boolean = False
        Private _isDisposed As Boolean = False

        Public Sub New(aiEngine As AIEngine.AIEngine)
            _aiEngine = aiEngine
            _viewModel = New SidebarViewModel(_aiEngine)
            _sidebarControl = New AISidebarWpf(_viewModel)
        End Sub

        ''' <summary>
        ''' Shows the sidebar.
        ''' </summary>
        Public Sub Show()
            If _isDisposed Then Return
            _isVisible = True
            ' In production, this would be registered as a custom task pane
            System.Diagnostics.Debug.WriteLine("Sidebar shown.")
        End Sub

        ''' <summary>
        ''' Hides the sidebar.
        ''' </summary>
        Public Sub Hide()
            _isVisible = False
            System.Diagnostics.Debug.WriteLine("Sidebar hidden.")
        End Sub

        ''' <summary>
        ''' Loads an email into the sidebar for processing.
        ''' </summary>
        Public Sub LoadEmail(email As EmailMessage)
            If _isDisposed Then Return
            _viewModel.SelectedEmail = email
            _sidebarControl.ClearResult()
            _sidebarControl.SetStatus($"Loaded: {email.Subject}")
        End Sub

        ''' <summary>
        ''' Returns whether the sidebar is currently visible.
        ''' </summary>
        Public ReadOnly Property IsVisible As Boolean
            Get
                Return _isVisible
            End Get
        End Property

        Public Sub Dispose() Implements IDisposable.Dispose
            If Not _isDisposed Then
                _sidebarControl = Nothing
                _viewModel = Nothing
                _isDisposed = True
            End If
        End Sub

    End Class

End Namespace