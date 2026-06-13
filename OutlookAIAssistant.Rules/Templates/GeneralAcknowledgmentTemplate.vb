Namespace OutlookAIAssistant.Rules.Templates

    ''' <summary>
    ''' General-purpose acknowledgment templates for common email scenarios
    ''' that don't fall under support or sales categories.
    ''' </summary>
    Public Class GeneralAcknowledgmentTemplate

        ''' <summary>
        ''' Acknowledges receipt of a general inquiry or message.
        ''' </summary>
        Public Shared Function GeneralReply(senderName As String, subject As String, recipientName As String) As String
            Return $"Hi {senderName},

Thank you for your message regarding '{subject}'.

I have received it and will get back to you as soon as possible. If your matter is urgent, please don't hesitate to follow up.

Best regards,

{recipientName}"
        End Function

        ''' <summary>
        ''' Confirms receipt of an attachment or document.
        ''' </summary>
        Public Shared Function AttachmentReceived(senderName As String, subject As String, recipientName As String) As String
            Return $"Hi {senderName},

I've received the documents you sent regarding '{subject}'. Thank you for providing them.

I will review the materials and follow up with any questions or next steps.

Best regards,

{recipientName}"
        End Function

        ''' <summary>
        ''' Acknowledges a meeting request or invitation.
        ''' </summary>
        Public Shared Function MeetingConfirmed(senderName As String, subject As String, meetingDate As String, recipientName As String) As String
            Return $"Hi {senderName},

Thank you for the invitation regarding '{subject}'.

I'm pleased to confirm my attendance for {meetingDate}. Please let me know if there's anything I need to prepare in advance.

Looking forward to it,

{recipientName}"
        End Function

        ''' <summary>
        ''' Responds to a forwarded email or introduction.
        ''' </summary>
        Public Shared Function IntroductionReply(senderName As String, introducerName As String, recipientName As String) As String
            Return $"Hi {senderName},

Thank you for reaching out. {introducerName} mentioned you'd be in touch, and I'm glad to connect.

I'd love to learn more about your work and discuss how we might collaborate. Please let me know what times work best for a quick call.

Best regards,

{recipientName}"
        End Function

    End Class

End Namespace