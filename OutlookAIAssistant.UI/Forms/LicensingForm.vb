Imports System.Windows.Forms

Namespace OutlookAIAssistant.UI.Forms

    ''' <summary>
    ''' Form for license activation and plan management.
    ''' </summary>
    Public Class LicensingForm
        Inherits Form

        Private WithEvents txtLicenseKey As TextBox
        Private WithEvents btnActivate As Button
        Private WithEvents lblStatus As Label
        Private WithEvents btnUpgrade As Button

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub InitializeComponent()
            Me.Text = "License Management"
            Me.Size = New System.Drawing.Size(450, 250)
            Me.StartPosition = FormStartPosition.CenterParent
            Me.FormBorderStyle = FormBorderStyle.FixedDialog

            Dim y As Integer = 20
            Dim controlX As Integer = 20
            Dim controlWidth As Integer = 400

            ' License info
            Dim lblCurrent = New Label() With {
                .Text = "Current Plan: Free",
                .Location = New Point(controlX, y),
                .Size = New Size(controlWidth, 25),
                .Font = New Font("Segoe UI", 12, FontStyle.Bold)
            }
            Controls.Add(lblCurrent)
            y += 40

            ' License key input
            Controls.Add(New Label() With {
                .Text = "License Key:",
                .Location = New Point(controlX, y),
                .Size = New Size(100, 25)
            })
            txtLicenseKey = New TextBox() With {
                .Location = New Point(controlX + 100, y),
                .Size = New Size(200, 25)
            }
            Controls.Add(txtLicenseKey)

            btnActivate = New Button() With {
                .Text = "Activate",
                .Location = New Point(controlX + 310, y),
                .Size = New Size(90, 25)
            }
            Controls.Add(btnActivate)
            y += 40

            ' Status
            lblStatus = New Label() With {
                .Text = "",
                .Location = New Point(controlX, y),
                .Size = New Size(controlWidth, 25),
                .ForeColor = Color.Green
            }
            Controls.Add(lblStatus)
            y += 40

            ' Upgrade button
            btnUpgrade = New Button() With {
                .Text = "Upgrade Plan...",
                .Location = New Point(controlX, y),
                .Size = New Size(120, 30)
            }
            Controls.Add(btnUpgrade)

            ' Close button
            Dim btnClose = New Button() With {
                .Text = "Close",
                .DialogResult = DialogResult.Cancel,
                .Location = New Point(controlX + 300, y),
                .Size = New Size(80, 30)
            }
            Controls.Add(btnClose)
        End Sub

        Private Sub OnActivateClick(sender As Object, e As EventArgs) Handles btnActivate.Click
            If String.IsNullOrEmpty(txtLicenseKey.Text) Then
                lblStatus.Text = "Please enter a license key."
                lblStatus.ForeColor = Color.Red
                Return
            End If

            lblStatus.Text = "Activating..."
            lblStatus.ForeColor = Color.Blue

            ' In production: call LicenseManager.ActivateLicenseAsync
            lblStatus.Text = "License activated successfully!"
            lblStatus.ForeColor = Color.Green
        End Sub

    End Class

End Namespace