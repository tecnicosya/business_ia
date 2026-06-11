Imports System.Windows.Forms

Namespace OutlookAIAssistant.UI.Forms

    ''' <summary>
    ''' Form for creating and editing automation rules.
    ''' </summary>
    Public Class RulesEditorForm
        Inherits Form

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub InitializeComponent()
            Me.Text = "Rules Editor"
            Me.Size = New System.Drawing.Size(500, 400)
            Me.StartPosition = FormStartPosition.CenterParent
        End Sub

    End Class

End Namespace