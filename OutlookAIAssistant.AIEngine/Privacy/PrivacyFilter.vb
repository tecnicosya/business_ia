Imports System.Text.RegularExpressions
Imports OutlookAIAssistant.Core.Models

Namespace OutlookAIAssistant.AIEngine.Privacy

    ''' <summary>
    ''' Applies privacy filters to email content before sending to AI providers.
    ''' Supports PII removal, anonymization, and on-premises routing decisions.
    ''' </summary>
    Public Class PrivacyFilter

        ' Common PII patterns
        Private Shared ReadOnly _emailPattern As String = "[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}"
        Private Shared ReadOnly _phonePattern As String = "\b[\+]?[\d\(\)\-\s]{7,15}\b"
        Private Shared ReadOnly _ssnPattern As String = "\b\d{3}-\d{2}-\d{4}\b"
        Private Shared ReadOnly _creditCardPattern As String = "\b(?:\d[ -]*?){13,16}\b"

        ''' <summary>
        ''' Applies the privacy filter based on the specified mode.
        ''' </summary>
        Public Function Apply(content As String, mode As PrivacyMode) As String
            If mode.AnonymizePII Then
                content = RemovePII(content)
            End If

            ' For on-premises, just return content as-is after optional PII removal
            ' (data never leaves the network in this mode)
            Return content
        End Function

        ''' <summary>
        ''' Removes or anonymizes personally identifiable information from text.
        ''' </summary>
        Public Function RemovePII(content As String) As String
            Dim result = content

            result = Regex.Replace(result, _emailPattern, "[EMAIL REDACTED]", RegexOptions.IgnoreCase)
            result = Regex.Replace(result, _phonePattern, "[PHONE REDACTED]")
            result = Regex.Replace(result, _ssnPattern, "[SSN REDACTED]")
            result = Regex.Replace(result, _creditCardPattern, "[CC REDACTED]")

            Return result
        End Function

        ''' <summary>
        ''' Determines whether content can be sent to the given provider based on privacy mode.
        ''' </summary>
        Public Function CanSendToProvider(mode As PrivacyMode, providerType As String) As Boolean
            ' On-premises mode: only allow local providers
            If mode.OnPremises Then
                Return providerType.Equals("Ollama", StringComparison.OrdinalIgnoreCase) OrElse
                       providerType.Equals("LM Studio", StringComparison.OrdinalIgnoreCase) OrElse
                       providerType.Equals("OpenAI Compatible", StringComparison.OrdinalIgnoreCase)
            End If

            ' Standard and anonymized modes: allow all providers
            Return True
        End Function

    End Class

End Namespace