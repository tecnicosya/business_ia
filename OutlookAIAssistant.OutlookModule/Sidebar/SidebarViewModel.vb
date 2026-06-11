Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports System.Threading.Tasks
Imports OutlookAIAssistant.Core.Models

Namespace OutlookAIAssistant.OutlookModule.Sidebar

    ''' <summary>
    ''' ViewModel for the AI sidebar. Binds to AISidebarWpf.xaml.
    ''' </summary>
    Public Class SidebarViewModel
        Implements INotifyPropertyChanged

        Private _selectedEmail As EmailMessage
        Private _aiEngine As AIEngine.AIEngine
        Private _resultText As String
        Private _statusText As String = "Ready"
        Private _isProcessing As Boolean = False

        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        Public Sub New()
        End Sub

        Public Sub New(aiEngine As AIEngine.AIEngine)
            _aiEngine = aiEngine
        End Sub

        Public ReadOnly Property AiEngine As AIEngine.AIEngine
            Get
                Return _aiEngine
            End Get
        End Property

        Public Property SelectedEmail As EmailMessage
            Get
                Return _selectedEmail
            End Get
            Set(value As EmailMessage)
                _selectedEmail = value
                OnPropertyChanged()
            End Set
        End Property

        Public Property ResultText As String
            Get
                Return _resultText
            End Get
            Set(value As String)
                _resultText = value
                OnPropertyChanged()
            End Set
        End Property

        Public Property StatusText As String
            Get
                Return _statusText
            End Get
            Set(value As String)
                _statusText = value
                OnPropertyChanged()
            End Set
        End Property

        Public Property IsProcessing As Boolean
            Get
                Return _isProcessing
            End Get
            Set(value As Boolean)
                _isProcessing = value
                OnPropertyChanged()
            End Set
        End Property

        ''' <summary>
        ''' Summarizes the selected email.
        ''' </summary>
        Public Async Function SummarizeAsync() As Task
            If _selectedEmail Is Nothing OrElse _aiEngine Is Nothing Then Return

            IsProcessing = True
            StatusText = "Summarizing..."
            ResultText = ""

            Try
                Dim response = Await _aiEngine.SummarizeAsync(
                    New AIRequest With {
                        .Content = _selectedEmail.Body,
                        .Subject = _selectedEmail.Subject,
                        .ProviderName = _aiEngine.GetConfiguredProviders().FirstOrDefault()
                    }
                )

                If response.IsSuccess Then
                    ResultText = response.Content
                Else
                    ResultText = $"Error: {response.ErrorMessage}"
                End If
            Catch ex As Exception
                ResultText = $"Error: {ex.Message}"
            Finally
                IsProcessing = False
                StatusText = "Ready"
            End Try
        End Function

        ''' <summary>
        ''' Generates a reply for the selected email.
        ''' </summary>
        Public Async Function GenerateReplyAsync() As Task
            If _selectedEmail Is Nothing OrElse _aiEngine Is Nothing Then Return

            IsProcessing = True
            StatusText = "Generating reply..."
            ResultText = ""

            Try
                Dim response = Await _aiEngine.GenerateReplyAsync(
                    New AIRequest With {
                        .Content = _selectedEmail.Body,
                        .Subject = _selectedEmail.Subject,
                        .SenderEmail = _selectedEmail.SenderEmail,
                        .ProviderName = _aiEngine.GetConfiguredProviders().FirstOrDefault()
                    }
                )

                If response.IsSuccess Then
                    ResultText = response.Content
                Else
                    ResultText = $"Error: {response.ErrorMessage}"
                End If
            Catch ex As Exception
                ResultText = $"Error: {ex.Message}"
            Finally
                IsProcessing = False
                StatusText = "Ready"
            End Try
        End Function

        ''' <summary>
        ''' Classifies the selected email.
        ''' </summary>
        Public Async Function ClassifyAsync() As Task
            If _selectedEmail Is Nothing OrElse _aiEngine Is Nothing Then Return

            IsProcessing = True
            StatusText = "Classifying..."
            ResultText = ""

            Try
                Dim classification = Await _aiEngine.ClassifyAsync(
                    New AIRequest With {
                        .Content = _selectedEmail.Body,
                        .Subject = _selectedEmail.Subject,
                        .ProviderName = _aiEngine.GetConfiguredProviders().FirstOrDefault()
                    }
                )

                If classification IsNot Nothing Then
                    ResultText = $"Category: {classification.Type}" & vbCrLf &
                                $"Sub-Category: {classification.SubCategory}" & vbCrLf &
                                $"Priority: {classification.Priority}" & vbCrLf &
                                $"Confidence: {classification.Confidence:P}" & vbCrLf &
                                $"Action Required: {classification.RequiresAction}" & vbCrLf &
                                $"Sentiment: {classification.Sentiment}"
                End If
            Catch ex As Exception
                ResultText = $"Error: {ex.Message}"
            Finally
                IsProcessing = False
                StatusText = "Ready"
            End Try
        End Function

        Protected Sub OnPropertyChanged(<CallerMemberName> Optional propertyName As String = Nothing)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
        End Sub

    End Class

End Namespace