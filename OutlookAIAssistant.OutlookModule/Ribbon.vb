Imports System.Runtime.InteropServices
Imports Office = Microsoft.Office.Core
Imports OutlookAIAssistant.OutlookModule.Sidebar

Namespace OutlookAIAssistant.OutlookModule

    ''' <summary>
    ''' Ribbon (Fluent UI) for the Outlook AI Assistant add-in.
    ''' Handles button clicks and UI state.
    ''' </summary>
    <ComVisible(True)>
    Public Class Ribbon
        Implements Office.IRibbonExtensibility

        Private ribbonUI As Office.IRibbonUI
        Private _sidebarVisible As Boolean = False

        Public Sub New()
        End Sub

        Public Function GetCustomUI(ribbonID As String) As String Implements Office.IRibbonExtensibility.GetCustomUI
            Return GetResourceText("OutlookAIAssistant.OutlookModule.Ribbon.xml")
        End Function

        #Region "Ribbon Callbacks"

        ''' <summary>
        ''' Called when the Ribbon loads.
        ''' </summary>
        Public Sub Ribbon_Load(ribbonUI As Office.IRibbonUI)
            Me.ribbonUI = ribbonUI
        End Sub

        ''' <summary>
        ''' Handles the Summarize button click.
        ''' </summary>
        Public Sub OnSummarizeClick(control As Office.IRibbonControl)
            Try
                Dim addIn = Globals.ThisAddIn
                Dim explorer = addIn.Application.ActiveExplorer()
                Dim selection = explorer.Selection

                If selection.Count > 0 Then
                    Dim mailItem = TryCast(selection(1), Microsoft.Office.Interop.Outlook.MailItem)
                    If mailItem IsNot Nothing Then
                        addIn.GetAIEngine().SummarizeAsync(
                            New Core.Models.AIRequest With {
                                .Content = mailItem.Body,
                                .Subject = mailItem.Subject,
                                .ProviderName = addIn.GetAIEngine().GetConfiguredProviders().FirstOrDefault()
                            }
                        )
                    End If
                End If
            Catch ex As Exception
                System.Windows.Forms.MessageBox.Show($"Error summarizing: {ex.Message}", "AI Assistant",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error)
            End Try
        End Sub

        ''' <summary>
        ''' Handles the Draft Reply button click.
        ''' </summary>
        Public Sub OnReplyClick(control As Office.IRibbonControl)
            Try
                Dim addIn = Globals.ThisAddIn
                Dim explorer = addIn.Application.ActiveExplorer()
                Dim selection = explorer.Selection

                If selection.Count > 0 Then
                    Dim mailItem = TryCast(selection(1), Microsoft.Office.Interop.Outlook.MailItem)
                    If mailItem IsNot Nothing Then
                        ' Create reply item
                        Dim replyItem = TryCast(mailItem.Reply(), Microsoft.Office.Interop.Outlook.MailItem)
                        If replyItem IsNot Nothing Then

                            Dim response = addIn.GetAIEngine().GenerateReplyAsync(
                                New Core.Models.AIRequest With {
                                    .Content = mailItem.Body,
                                    .Subject = mailItem.Subject,
                                    .SenderEmail = mailItem.SenderEmailAddress,
                                    .ProviderName = addIn.GetAIEngine().GetConfiguredProviders().FirstOrDefault()
                                }
                            ).Result

                            If response.IsSuccess Then
                                replyItem.Body = response.Content & vbCrLf & vbCrLf & replyItem.Body
                                replyItem.Display(False)
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                System.Windows.Forms.MessageBox.Show($"Error drafting reply: {ex.Message}", "AI Assistant",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error)
            End Try
        End Sub

        ''' <summary>
        ''' Handles the Categorize button click.
        ''' </summary>
        Public Sub OnClassifyClick(control As Office.IRibbonControl)
            Try
                Dim addIn = Globals.ThisAddIn
                Dim explorer = addIn.Application.ActiveExplorer()
                Dim selection = explorer.Selection

                If selection.Count > 0 Then
                    Dim mailItem = TryCast(selection(1), Microsoft.Office.Interop.Outlook.MailItem)
                    If mailItem IsNot Nothing Then
                        Dim classification = addIn.GetAIEngine().ClassifyAsync(
                            New Core.Models.AIRequest With {
                                .Content = mailItem.Body,
                                .Subject = mailItem.Subject,
                                .ProviderName = addIn.GetAIEngine().GetConfiguredProviders().FirstOrDefault()
                            }
                        ).Result

                        If classification IsNot Nothing Then
                            System.Windows.Forms.MessageBox.Show(
                                $"Category: {classification.Type}" & vbCrLf &
                                $"Priority: {classification.Priority}" & vbCrLf &
                                $"Confidence: {classification.Confidence:P}" & vbCrLf &
                                $"Action Required: {classification.RequiresAction}",
                                "Email Classification",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Information)
                        End If
                    End If
                End If
            Catch ex As Exception
                System.Windows.Forms.MessageBox.Show($"Error classifying: {ex.Message}", "AI Assistant",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error)
            End Try
        End Sub

        ''' <summary>
        ''' Handles the Translate button click.
        ''' </summary>
        Public Sub OnTranslateClick(control As Office.IRibbonControl)
            ' Opens the translation dialog
            Dim form = New UI.Forms.ProviderConfigForm()
            form.ShowDialog()
        End Sub

        ''' <summary>
        ''' Handles the Proofread button click.
        ''' </summary>
        Public Sub OnProofreadClick(control As Office.IRibbonControl)
            Try
                Dim addIn = Globals.ThisAddIn
                Dim inspector = addIn.Application.ActiveInspector()
                If inspector IsNot Nothing AndAlso inspector.CurrentItem IsNot Nothing Then
                    Dim mailItem = TryCast(inspector.CurrentItem, Microsoft.Office.Interop.Outlook.MailItem)
                    If mailItem IsNot Nothing AndAlso Not String.IsNullOrEmpty(mailItem.Body) Then
                        Dim response = addIn.GetAIEngine().ProofreadAsync(
                            New Core.Models.AIRequest With {
                                .Content = mailItem.Body,
                                .ProviderName = addIn.GetAIEngine().GetConfiguredProviders().FirstOrDefault()
                            }
                        ).Result

                        If response.IsSuccess Then
                            mailItem.Body = response.Content
                        End If
                    End If
                End If
            Catch ex As Exception
                System.Windows.Forms.MessageBox.Show($"Error proofreading: {ex.Message}", "AI Assistant",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error)
            End Try
        End Sub

        ''' <summary>
        ''' Handles the Smart Reply button click.
        ''' </summary>
        Public Sub OnSmartReplyClick(control As Office.IRibbonControl)
            ' Opens sidebar with smart reply options
            OnToggleSidebarClick(control)
        End Sub

        ''' <summary>
        ''' Handles the Settings button click.
        ''' </summary>
        Public Sub OnSettingsClick(control As Office.IRibbonControl)
            Dim form = New UI.Forms.SettingsForm()
            form.ShowDialog()
        End Sub

        ''' <summary>
        ''' Toggles the AI sidebar visibility.
        ''' </summary>
        Public Sub OnToggleSidebarClick(control As Office.IRibbonControl)
            Dim addIn = Globals.ThisAddIn
            _sidebarVisible = Not _sidebarVisible
            If _sidebarVisible Then
                addIn.GetSidebarManager().Show()
            Else
                addIn.GetSidebarManager().Hide()
            End If
            ribbonUI.InvalidateControl(control.Id)
        End Sub

        ''' <summary>
        ''' Gets the pressed state for toggle buttons.
        ''' </summary>
        Public Function GetSidebarPressed(control As Office.IRibbonControl) As Boolean
            Return _sidebarVisible
        End Function

        #End Region

        #Region "Helpers"

        Private Shared Function GetResourceText(resourceName As String) As String
            Dim assembly = System.Reflection.Assembly.GetExecutingAssembly()
            Dim resourceNames = assembly.GetManifestResourceNames()
            For Each name In resourceNames
                If name.EndsWith("Ribbon.xml") Then
                    Using stream = assembly.GetManifestResourceStream(name)
                        Using reader = New System.IO.StreamReader(stream)
                            Return reader.ReadToEnd()
                        End Using
                    End Using
                End If
            Next
            Return ""
        End Function

        #End Region

    End Class

End Namespace