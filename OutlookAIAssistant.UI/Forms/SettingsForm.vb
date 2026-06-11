Imports System.Windows.Forms

Namespace OutlookAIAssistant.UI.Forms

    ''' <summary>
    ''' Main settings form for configuring AI providers, rules, licensing, and privacy.
    ''' </summary>
    Public Class SettingsForm
        Inherits Form

        Private WithEvents tabControl As TabControl
        Private WithEvents btnSave As Button
        Private WithEvents btnCancel As Button

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub InitializeComponent()
            Me.Text = "Outlook AI Assistant - Settings"
            Me.Size = New System.Drawing.Size(600, 450)
            Me.StartPosition = FormStartPosition.CenterParent
            Me.FormBorderStyle = FormBorderStyle.FixedDialog
            Me.MaximizeBox = False
            Me.MinimizeBox = False

            tabControl = New TabControl() With {
                .Dock = DockStyle.Fill
            }

            ' Provider configuration tab
            tabControl.TabPages.Add(New TabPage("AI Providers"))
            ' Rules tab
            tabControl.TabPages.Add(New TabPage("Rules & Automation"))
            ' Privacy tab
            tabControl.TabPages.Add(New TabPage("Privacy & Security"))
            ' Licensing tab
            tabControl.TabPages.Add(New TabPage("Licensing"))
            ' Analytics tab
            tabControl.TabPages.Add(New TabPage("Analytics"))

            btnSave = New Button() With {
                .Text = "Save",
                .DialogResult = DialogResult.OK,
                .Location = New System.Drawing.Point(420, 380),
                .Size = New System.Drawing.Size(80, 30)
            }

            btnCancel = New Button() With {
                .Text = "Cancel",
                .DialogResult = DialogResult.Cancel,
                .Location = New System.Drawing.Point(510, 380),
                .Size = New System.Drawing.Size(80, 30)
            }

            Me.Controls.Add(tabControl)
            Me.Controls.Add(btnSave)
            Me.Controls.Add(btnCancel)
        End Sub

        Private Sub OnSaveClick(sender As Object, e As EventArgs) Handles btnSave.Click
            ' Save configuration
            Me.DialogResult = DialogResult.OK
            Me.Close()
        End Sub

    End Class

End Namespace