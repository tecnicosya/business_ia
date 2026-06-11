Namespace OutlookAIAssistant.Rules.Templates

    ''' <summary>
    ''' Support reply templates for common support scenarios.
    ''' </summary>
    Public Class SupportTemplate

        ''' <summary>
        ''' Acknowledgment template — confirms receipt of a support request.
        ''' </summary>
        Public Shared Function Acknowledgment(senderName As String, subject As String) As String
            Return $"Hi {senderName},

Thank you for reaching out to us regarding '{subject}'.

We have received your request and one of our support team members will get back to you within 24 hours.

If you have any additional information to add, please reply to this email.

Best regards,
Support Team"
        End Function

        ''' <summary>
        ''' Resolved template — notifies the user their issue has been resolved.
        ''' </summary>
        Public Shared Function Resolved(senderName As String, subject As String) As String
            Return $"Hi {senderName},

We're writing to let you know that your support request regarding '{subject}' has been resolved.

If you're satisfied with the resolution, no further action is needed. If you have any additional questions, please don't hesitate to reach out.

Best regards,
Support Team"
        End Function

        ''' <summary>
        ''' Follow-up template — requests additional information.
        ''' </summary>
        Public Shared Function FollowUp(senderName As String, subject As String, details As String) As String
            Return $"Hi {senderName},

Regarding your request '{subject}', we need some additional information to help resolve your issue:

{details}

Please provide the requested information at your earliest convenience.

Best regards,
Support Team"
        End Function

    End Class

End Namespace