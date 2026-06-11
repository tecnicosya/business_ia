Imports System.Windows.Forms

Namespace OutlookAIAssistant.UI.Forms

    ''' <summary>
    ''' Form for configuring AI provider settings (API key, endpoint, model, etc.).
    ''' </summary>
    Public Class ProviderConfigForm
        Inherits Form

        Private WithEvents txtName As TextBox
        Private WithEvents cmbProviderType As ComboBox
        Private WithEvents txtApiEndpoint As TextBox
        Private WithEvents txtApiKey As TextBox
        Private WithEvents txtModel As TextBox
        Private WithEvents numMaxTokens As NumericUpDown
        Private WithEvents numTemperature As NumericUpDown
        Private WithEvents chkEnabled As CheckBox
        Private WithEvents btnSave As Button
        Private WithEvents btnTest As Button
        Private WithEvents btnCancel As Button

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub InitializeComponent()
            Me.Text = "AI Provider Configuration"
            Me.Size = New System.Drawing.Size(450, 380)
            Me.StartPosition = FormStartPosition.CenterParent
            Me.FormBorderStyle = FormBorderStyle.FixedDialog

            Dim y As Integer = 15
            Dim labelX As Integer = 15
            Dim controlX As Integer = 130
            Dim controlWidth As Integer = 290
            Dim height As Integer = 25
            Dim spacing As Integer = 30

            ' Name
            Controls.Add(New Label With {.Text = "Name:", .Location = New Point(labelX, y), .Size = New Size(100, height)})
            txtName = New TextBox With {.Location = New Point(controlX, y), .Size = New Size(controlWidth, height)}
            Controls.Add(txtName)
            y += spacing

            ' Provider Type
            Controls.Add(New Label With {.Text = "Provider:", .Location = New Point(labelX, y), .Size = New Size(100, height)})
            cmbProviderType = New ComboBox With {
                .Location = New Point(controlX, y),
                .Size = New Size(controlWidth, height),
                .DropDownStyle = ComboBoxStyle.DropDownList
            }
            cmbProviderType.Items.AddRange(New String() {
                "OpenAI", "DeepSeek", "Gemini", "Anthropic", "Ollama", "LM Studio", "OpenRouter", "OpenAI Compatible"
            })
            Controls.Add(cmbProviderType)
            y += spacing

            ' API Endpoint
            Controls.Add(New Label With {.Text = "API Endpoint:", .Location = New Point(labelX, y), .Size = New Size(100, height)})
            txtApiEndpoint = New TextBox With {.Location = New Point(controlX, y), .Size = New Size(controlWidth, height)}
            Controls.Add(txtApiEndpoint)
            y += spacing

            ' API Key
            Controls.Add(New Label With {.Text = "API Key:", .Location = New Point(labelX, y), .Size = New Size(100, height)})
            txtApiKey = New TextBox With {
                .Location = New Point(controlX, y),
                .Size = New Size(controlWidth, height),
                .PasswordChar = "*"c
            }
            Controls.Add(txtApiKey)
            y += spacing

            ' Model
            Controls.Add(New Label With {.Text = "Model:", .Location = New Point(labelX, y), .Size = New Size(100, height)})
            txtModel = New TextBox With {.Location = New Point(controlX, y), .Size = New Size(controlWidth, height)}
            Controls.Add(txtModel)
            y += spacing

            ' Max Tokens
            Controls.Add(New Label With {.Text = "Max Tokens:", .Location = New Point(labelX, y), .Size = New Size(100, height)})
            numMaxTokens = New NumericUpDown With {
                .Location = New Point(controlX, y),
                .Size = New Size(100, height),
                .Minimum = 128,
                .Maximum = 32768,
                .Value = 2048
            }
            Controls.Add(numMaxTokens)
            y += spacing

            ' Temperature
            Controls.Add(New Label With {.Text = "Temperature:", .Location = New Point(labelX, y), .Size = New Size(100, height)})
            numTemperature = New NumericUpDown With {
                .Location = New Point(controlX, y),
                .Size = New Size(100, height),
                .DecimalPlaces = 1,
                .Increment = 0.1D,
                .Minimum = 0D,
                .Maximum = 2D,
                .Value = 0.7D
            }
            Controls.Add(numTemperature)
            y += spacing

            ' Enabled
            chkEnabled = New CheckBox With {
                .Text = "Enabled",
                .Location = New Point(controlX, y),
                .Size = New Size(100, height),
                .Checked = True
            }
            Controls.Add(chkEnabled)
            y += spacing + 10

            ' Buttons
            btnTest = New Button With {
                .Text = "Test Connection",
                .Location = New Point(controlX, y),
                .Size = New Size(120, 30)
            }
            Controls.Add(btnTest)

            btnSave = New Button With {
                .Text = "Save",
                .DialogResult = DialogResult.OK,
                .Location = New Point(240, y),
                .Size = New Size(80, 30)
            }
            Controls.Add(btnSave)

            btnCancel = New Button With {
                .Text = "Cancel",
                .DialogResult = DialogResult.Cancel,
                .Location = New Point(330, y),
                .Size = New Size(80, 30)
            }
            Controls.Add(btnCancel)
        End Sub

        Private Sub OnSaveClick(sender As Object, e As EventArgs) Handles btnSave.Click
            ' Validate and save provider configuration
            Me.DialogResult = DialogResult.OK
            Me.Close()
        End Sub

    End Class

End Namespace