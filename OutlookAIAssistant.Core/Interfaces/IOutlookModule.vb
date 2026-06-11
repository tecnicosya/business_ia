Imports OutlookAIAssistant.Core.Models

Namespace OutlookAIAssistant.Core.Interfaces

    ''' <summary>
    ''' Interface for Outlook add-in lifecycle management: startup, shutdown, ribbon, event wiring.
    ''' </summary>
    Public Interface IOutlookModule

        ''' <summary>
        ''' Called on add-in startup. Initializes subsystems and wires Outlook events.
        ''' </summary>
        Sub Initialize()

        ''' <summary>
        ''' Called on add-in shutdown. Disposes resources and unwires events.
        ''' </summary>
        Sub Shutdown()

        ''' <summary>
        ''' Returns the current Outlook Application object.
        ''' </summary>
        ReadOnly Property Application As Object

        ''' <summary>
        ''' Event raised when a new email item is added to a folder.
        ''' </summary>
        Event EmailReceived As Action(Of EmailMessage)

        ''' <summary>
        ''' Event raised when an email is about to be sent.
        ''' </summary>
        Event EmailSending As Func(Of EmailMessage, Boolean)

        ''' <summary>
        ''' Event raised when an email inspector (open window) is created.
        ''' </summary>
        Event InspectorOpened As Action(Of EmailMessage)

    End Interface

End Namespace