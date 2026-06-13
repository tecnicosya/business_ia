Namespace OutlookAIAssistant.Rules.Templates

    ''' <summary>
    ''' Sales-oriented email reply templates for common sales scenarios.
    ''' These provide structured, professional responses for sales outreach,
    ''' follow-ups, and lead nurturing.
    ''' </summary>
    Public Class SalesTemplate

        ''' <summary>
        ''' Follow-up template — sent after initial outreach or meeting.
        ''' </summary>
        Public Shared Function FollowUp(senderName As String, subject As String, recipientName As String) As String
            Return $"Hi {senderName},

Thank you for your interest in our services. I wanted to follow up on our previous conversation regarding '{subject}'.

I'd love to schedule a quick call to discuss how we can help address your needs. Please let me know what time works best for you.

Looking forward to connecting,

{recipientName}"
        End Function

        ''' <summary>
        ''' Proposal response template — acknowledges receipt of a proposal and sets expectations.
        ''' </summary>
        Public Shared Function ProposalResponse(senderName As String, proposalName As String, recipientName As String) As String
            Return $"Hi {senderName},

Thank you for reviewing our proposal for '{proposalName}'.

I'm happy to answer any questions you may have about the proposed solution, timeline, or pricing. Please feel free to reach out at any time.

I look forward to your feedback.

Best regards,

{recipientName}"
        End Function

        ''' <summary>
        ''' Meeting request template — proposes a meeting time and agenda.
        ''' </summary>
        Public Shared Function MeetingRequest(senderName As String, subject As String, suggestedDate As String, recipientName As String) As String
            Return $"Hi {senderName},

I hope this message finds you well. I'd like to schedule a meeting to discuss '{subject}'.

Would {suggestedDate} work for you? If not, please let me know what dates and times would be more convenient.

Looking forward to our conversation,

{recipientName}"
        End Function

        ''' <summary>
        ''' Thank-you template — for post-meeting or post-purchase follow-up.
        ''' </summary>
        Public Shared Function ThankYou(senderName As String, subject As String, recipientName As String) As String
            Return $"Hi {senderName},

Thank you for your time earlier. I really enjoyed our conversation about '{subject}'.

As a next step, I'll prepare the materials we discussed and send them over shortly. In the meantime, please don't hesitate to reach out if anything comes to mind.

Best regards,

{recipientName}"
        End Function

    End Class

End Namespace