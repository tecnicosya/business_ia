Imports OutlookAIAssistant.Core.Enums

Namespace OutlookAIAssistant.Core.Models

    ''' <summary>
    ''' Represents an email message processed by the add-in.
    ''' Maps from Outlook MailItem properties.
    ''' </summary>
    Public Class EmailMessage
        ''' <summary>Outlook EntryID of the mail item.</summary>
        Public Property Id As String

        ''' <summary>Email subject line.</summary>
        Public Property Subject As String

        ''' <summary>Plain-text body content.</summary>
        Public Property Body As String

        ''' <summary>HTML body content, if available.</summary>
        Public Property BodyHtml As String

        ''' <summary>Sender email address.</summary>
        Public Property SenderEmail As String

        ''' <summary>Sender display name.</summary>
        Public Property SenderName As String

        ''' <summary>Semicolon-separated list of To recipients.</summary>
        Public Property ToRecipients As String

        ''' <summary>Semicolon-separated list of CC recipients.</summary>
        Public Property CcRecipients As String

        ''' <summary>Date and time the email was received.</summary>
        Public Property ReceivedTime As DateTime

        ''' <summary>Date and time the email was sent.</summary>
        Public Property SentTime As DateTime?

        ''' <summary>Email size in bytes.</summary>
        Public Property Size As Long

        ''' <summary>Conversation index (thread identifier).</summary>
        Public Property ConversationId As String

        ''' <summary>Whether the email has been read.</summary>
        Public Property IsRead As Boolean

        ''' <summary>Whether the email has attachments.</summary>
        Public Property HasAttachments As Boolean

        ''' <summary>Attachment filenames, if any.</summary>
        Public Property AttachmentNames As List(Of String)

        ''' <summary>Categories assigned to this email by Outlook or the AI.</summary>
        Public Property Categories As List(Of String)

        ''' <summary>Computed classification result.</summary>
        Public Property Classification As Classification

        ''' <summary>Flag to indicate this email was processed by the AI engine.</summary>
        Public Property IsAICategorized As Boolean
    End Class

End Namespace