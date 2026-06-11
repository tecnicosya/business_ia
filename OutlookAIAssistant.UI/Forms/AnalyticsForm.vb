Imports System.Windows.Forms

Namespace OutlookAIAssistant.UI.Forms

    ''' <summary>
    ''' Form displaying analytics and usage statistics.
    ''' </summary>
    Public Class AnalyticsForm
        Inherits Form

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub InitializeComponent()
            Me.Text = "Analytics & Usage"
            Me.Size = New System.Drawing.Size(500, 400)
            Me.StartPosition = FormStartPosition.CenterParent
        End Sub

    End Class

End Namespace