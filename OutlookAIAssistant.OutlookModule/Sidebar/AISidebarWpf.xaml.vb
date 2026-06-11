Namespace OutlookAIAssistant.OutlookModule.Sidebar

    ''' <summary>
    ''' Code-behind for the WPF AI sidebar user control.
    ''' </summary>
    Public Class AISidebarWpf

        Private _viewModel As SidebarViewModel

        Public Sub New()
            InitializeComponent()
            _viewModel = New SidebarViewModel()
            DataContext = _viewModel
            WireEvents()
        End Sub

        Public Sub New(viewModel As SidebarViewModel)
            InitializeComponent()
            _viewModel = viewModel
            DataContext = _viewModel
            WireEvents()
        End Sub

        Private Sub WireEvents()
            AddHandler btnSummarize.Click, Sub(s, e)
                _viewModel?.SummarizeAsync()
            End Sub
            AddHandler btnReply.Click, Sub(s, e)
                _viewModel?.GenerateReplyAsync()
            End Sub
            AddHandler btnClassify.Click, Sub(s, e)
                _viewModel?.ClassifyAsync()
            End Sub
        End Sub

        ''' <summary>
        ''' Updates the displayed result text.
        ''' </summary>
        Public Sub SetResult(text As String)
            If Not String.IsNullOrEmpty(text) Then
                txtResult.Text = text
            End If
        End Sub

        ''' <summary>
        ''' Updates the status text.
        ''' </summary>
        Public Sub SetStatus(text As String)
            txtStatus.Text = text
        End Sub

        ''' <summary>
        ''' Clears the result display.
        ''' </summary>
        Public Sub ClearResult()
            txtResult.Text = ""
        End Sub

    End Class

End Namespace