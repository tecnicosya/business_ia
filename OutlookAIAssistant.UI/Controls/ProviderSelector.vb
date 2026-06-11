Imports System.Windows.Forms

Namespace OutlookAIAssistant.UI.Controls

    ''' <summary>
    ''' User control for selecting and configuring AI providers.
    ''' </summary>
    Public Class ProviderSelector
        Inherits UserControl

        Private WithEvents cmbProviders As ComboBox
        Private WithEvents btnConfigure As Button

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub InitializeComponent()
            Me.Size = New System.Drawing.Size(300, 30)

            cmbProviders = New ComboBox() With {
                .Location = New Point(0, 0),
                .Size = New Size(200, 25),
                .DropDownStyle = ComboBoxStyle.DropDownList
            }
            cmbProviders.Items.Add("No providers configured")

            btnConfigure = New Button() With {
                .Text = "Configure...",
                .Location = New Point(210, 0),
                .Size = New Size(85, 25)
            }

            Controls.Add(cmbProviders)
            Controls.Add(btnConfigure)
        End Sub

        Private Sub OnConfigureClick(sender As Object, e As EventArgs) Handles btnConfigure.Click
            Dim form = New Forms.ProviderConfigForm()
            form.ShowDialog(Me.ParentForm)
        End Sub

        ''' <summary>
        ''' Loads providers into the selector.
        ''' </summary>
        Public Sub LoadProviders(providerNames As IReadOnlyList(Of String))
            cmbProviders.Items.Clear()
            If providerNames Is Nothing OrElse providerNames.Count = 0 Then
                cmbProviders.Items.Add("No providers configured")
            Else
                For Each name In providerNames
                    cmbProviders.Items.Add(name)
                Next
                cmbProviders.SelectedIndex = 0
            End If
        End Sub

        ''' <summary>
        ''' Gets the selected provider name.
        ''' </summary>
        Public ReadOnly Property SelectedProvider As String
            Get
                If cmbProviders.SelectedIndex >= 0 Then
                    Return cmbProviders.SelectedItem.ToString()
                End If
                Return Nothing
            End Get
        End Property

    End Class

End Namespace