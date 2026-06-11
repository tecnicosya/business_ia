Namespace OutlookAIAssistant.AIEngine.Prompts

    ''' <summary>
    ''' Manages prompt templates for AI operations.
    ''' Supports built-in prompts and loading custom .prompt files.
    ''' </summary>
    Public Class PromptManager

        Private ReadOnly _promptsFolder As String

        Public Sub New()
            Dim baseDir = AppDomain.CurrentDomain.BaseDirectory
            _promptsFolder = System.IO.Path.Combine(baseDir, "Prompts")
            If Not System.IO.Directory.Exists(_promptsFolder) Then
                System.IO.Directory.CreateDirectory(_promptsFolder)
            End If
        End Sub

        ''' <summary>Builds a prompt for email summarization.</summary>
        Public Function BuildSummaryPrompt(emailContent As String, subject As String) As String
            Return $"You are an AI email assistant. Summarize the following email concisely.

Subject: {subject}

Email Content:
{emailContent}

Provide a brief summary (2-3 sentences) capturing the key points and any action items."
        End Function

        ''' <summary>Builds a prompt for generating a reply to an email.</summary>
        Public Function BuildReplyPrompt(emailContent As String, subject As String, senderEmail As String, Optional instructions As String = Nothing) As String
            Dim instructionText = If(String.IsNullOrEmpty(instructions), "", $"
Additional Instructions: {instructions}")

            Return $"You are an AI email assistant. Draft a professional reply to the following email.

Original Subject: {subject}
Original Sender: {senderEmail}

Original Email:
{emailContent}{instructionText}

Write a polite, professional reply that addresses the key points. Keep it concise."
        End Function

        ''' <summary>Builds a prompt for translating email content.</summary>
        Public Function BuildTranslationPrompt(emailContent As String, targetLanguage As String) As String
            Return $"Translate the following email content to {targetLanguage}. Preserve the original tone and intent.

---
{emailContent}
---
Provide only the translated text."
        End Function

        ''' <summary>Builds a prompt for classifying an email.</summary>
        Public Function BuildClassificationPrompt(emailContent As String, subject As String) As String
            Return $"Classify the following email into one of these categories: Work, Personal, Newsletter, Marketing, Social, Meeting, Task, Support, Finance, Shipping, Security, Notification, Important, Spam, Internal, Project.

Email Subject: {subject}

Email Content:
{emailContent}

Respond in JSON format:
{{
  ""type"": ""category_name"",
  ""subCategory"": ""specific_tag"",
  ""priority"": ""high|normal|low"",
  ""requiresAction"": true/false,
  ""sentiment"": ""positive|neutral|negative|urgent"",
  ""detectedLanguage"": ""language_name"",
  ""confidence"": 0.0-1.0,
  ""explanation"": ""brief reason""
}}"
        End Function

        ''' <summary>Builds a prompt for proofreading text.</summary>
        Public Function BuildProofreadPrompt(text As String) As String
            Return $"Proofread the following email text. Fix grammar, spelling, and punctuation errors while preserving the original meaning and tone.

Text:
{text}

Provide the corrected version only."
        End Function

        ''' <summary>Loads a custom prompt from a .prompt file.</summary>
        Public Function LoadCustomPrompt(promptName As String) As String
            Dim filePath = System.IO.Path.Combine(_promptsFolder, $"{promptName}.prompt")
            If System.IO.File.Exists(filePath) Then
                Return System.IO.File.ReadAllText(filePath)
            End If
            Return Nothing
        End Function

        ''' <summary>Saves a custom prompt template.</summary>
        Public Sub SaveCustomPrompt(promptName As String, content As String)
            Dim filePath = System.IO.Path.Combine(_promptsFolder, $"{promptName}.prompt")
            System.IO.File.WriteAllText(filePath, content)
        End Sub
    End Class

End Namespace