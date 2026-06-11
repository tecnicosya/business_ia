Imports System.Runtime.InteropServices
Imports Microsoft.Office.Core
Imports Office = Microsoft.Office.Core

Namespace OutlookAIAssistant.OutlookModule

    ''' <summary>
    ''' Main add-in class for the Outlook AI Assistant.
    ''' Manages startup/shutdown lifecycle, wires Outlook events, and initializes subsystems.
    ''' </summary>
    <ComVisible(True)>
    Public Class ThisAddIn

        Private _aiEngine As AIEngine.AIEngine
        Private _rulesEngine As Rules.RulesEngine
        Private _securityManager As Security.Encryption.DPAPIEncryption
        Private _licenseManager As Security.Licensing.LicenseManager
        Private _sidebarManager As Sidebar.SidebarManager
        Private _taskPaneManager As TaskPane.TaskPaneManager
        Private _itemAddHandler As EventHandlers.ItemAddHandler
        Private _itemSendHandler As EventHandlers.ItemSendHandler
        Private _newInspectorHandler As EventHandlers.NewInspectorHandler
        Private _itemChangeHandler As EventHandlers.ItemChangeHandler
        Private _ribbon As Ribbon
        Private _isInitialized As Boolean = False

        Private outlookApp As Microsoft.Office.Interop.Outlook.Application

        ''' <summary>
        ''' Gets the Outlook Application object.
        ''' </summary>
        Public ReadOnly Property Application As Microsoft.Office.Interop.Outlook.Application
            Get
                Return outlookApp
            End Get
        End Property

        ''' <summary>
        ''' Called on add-in startup. Initializes components and wires events.
        ''' </summary>
        Private Sub ThisAddIn_Startup() Handles Me.Startup
            Try
                outlookApp = Globals.ThisAddIn.Application

                ' Initialize subsystems
                _aiEngine = New AIEngine.AIEngine()
                _rulesEngine = New Rules.RulesEngine()
                _securityManager = New Security.Encryption.DPAPIEncryption()
                _licenseManager = New Security.Licensing.LicenseManager()

                ' Initialize event handlers
                _itemAddHandler = New EventHandlers.ItemAddHandler(outlookApp, _aiEngine, _rulesEngine)
                _itemSendHandler = New EventHandlers.ItemSendHandler(outlookApp, _aiEngine)
                _newInspectorHandler = New EventHandlers.NewInspectorHandler(outlookApp, _aiEngine)
                _itemChangeHandler = New EventHandlers.ItemChangeHandler(outlookApp)

                ' Wire Outlook events
                AddHandler outlookApp.NewMailEx, AddressOf _itemAddHandler.OnNewMailEx
                AddHandler outlookApp.ItemSend, AddressOf _itemSendHandler.OnItemSend
                AddHandler outlookApp.Inspectors.NewInspector, AddressOf _newInspectorHandler.OnNewInspector

                ' Wire application-level events for item changes
                AddHandler outlookApp.AdvancedSearchComplete, AddressOf _itemChangeHandler.OnAdvancedSearchComplete

                ' Initialize sidebar and task pane
                _sidebarManager = New Sidebar.SidebarManager(_aiEngine)
                _taskPaneManager = New TaskPane.TaskPaneManager()

                _isInitialized = True

                System.Diagnostics.Debug.WriteLine("Outlook AI Assistant add-in initialized successfully.")
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine($"Outlook AI Assistant initialization error: {ex.Message}")
            End Try
        End Sub

        ''' <summary>
        ''' Called on add-in shutdown. Cleans up resources.
        ''' </summary>
        Private Sub ThisAddIn_Shutdown() Handles Me.Shutdown
            Try
                If Not _isInitialized Then Return

                ' Unwire events
                RemoveHandler outlookApp.NewMailEx, AddressOf _itemAddHandler.OnNewMailEx
                RemoveHandler outlookApp.ItemSend, AddressOf _itemSendHandler.OnItemSend
                RemoveHandler outlookApp.Inspectors.NewInspector, AddressOf _newInspectorHandler.OnNewInspector
                RemoveHandler outlookApp.AdvancedSearchComplete, AddressOf _itemChangeHandler.OnAdvancedSearchComplete

                ' Dispose managed resources
                _taskPaneManager?.Dispose()
                _sidebarManager?.Dispose()

                ' Release COM objects
                Marshal.ReleaseComObject(outlookApp)
                outlookApp = Nothing

                _isInitialized = False
                System.Diagnostics.Debug.WriteLine("Outlook AI Assistant add-in shut down successfully.")
            Catch ex As Exception
                System.Diagnostics.Debug.WriteLine($"Outlook AI Assistant shutdown error: {ex.Message}")
            End Try
        End Sub

        ''' <summary>
        ''' Returns the AI engine instance for ribbon/UI interactions.
        ''' </summary>
        Public Function GetAIEngine() As AIEngine.AIEngine
            Return _aiEngine
        End Function

        ''' <summary>
        ''' Returns the rules engine instance.
        ''' </summary>
        Public Function GetRulesEngine() As Rules.RulesEngine
            Return _rulesEngine
        End Function

        ''' <summary>
        ''' Returns the license manager instance.
        ''' </summary>
        Public Function GetLicenseManager() As Security.Licensing.LicenseManager
            Return _licenseManager
        End Function

        ''' <summary>
        ''' Returns the sidebar manager instance.
        ''' </summary>
        Public Function GetSidebarManager() As Sidebar.SidebarManager
            Return _sidebarManager
        End Function

        #Region "VSTO generated code"

        ''' <summary>
        ''' Required method for Designer support — do not modify.
        ''' </summary>
        Private Sub InitializeComponent()
            Me.Name = "OutlookAIAssistant.OutlookModule"
        End Sub

        #End Region

    End Class

End Namespace