Imports OutlookAIAssistant.Core.Interfaces
Imports OutlookAIAssistant.Security.Data

Namespace OutlookAIAssistant.Security.Analytics

    ''' <summary>
    ''' Tracks usage analytics locally: emails processed, time saved, responses generated,
    ''' tokens consumed, and provider usage. Data is stored in a local SQLite database.
    ''' Supports opt-in anonymous telemetry for product improvement.
    ''' </summary>
    Public Class AnalyticsService
        Implements IAnalyticsService

        Private ReadOnly _db As DatabaseHelper
        Private _analyticsEnabled As Boolean

        Public Sub New(Optional analyticsEnabled As Boolean = True)
            _db = New DatabaseHelper()
            _analyticsEnabled = analyticsEnabled
        End Sub

        ''' <summary>
        ''' Gets or sets whether analytics collection is enabled.
        ''' </summary>
        Public Property IsAnalyticsEnabled As Boolean
            Get
                Return _analyticsEnabled
            End Get
            Set(value As Boolean)
                _analyticsEnabled = value
            End Set
        End Property

        ''' <summary>
        ''' Tracks an event with optional properties.
        ''' Only records if analytics are enabled.
        ''' </summary>
        Public Sub TrackEvent(eventName As String, Optional properties As IDictionary(Of String, Object) = Nothing) Implements IAnalyticsService.TrackEvent
            If Not _analyticsEnabled Then Return
            If String.IsNullOrEmpty(eventName) Then Return

            Try
                Dim propertiesJson As String = Nothing
                If properties IsNot Nothing AndAlso properties.Count > 0 Then
                    propertiesJson = Newtonsoft.Json.JsonConvert.SerializeObject(properties)
                End If

                _db.Execute(
                    "INSERT INTO EventLog (EventName, Properties, CreatedDate) VALUES (@Name, @Props, @Date)",
                    Sub(cmd)
                        cmd.Parameters.AddWithValue("@Name", eventName)
                        cmd.Parameters.AddWithValue("@Props", If(propertiesJson, CObj(DBNull.Value)))
                        cmd.Parameters.AddWithValue("@Date", DateTime.UtcNow.ToString("O"))
                    End Sub
                )
            Catch
                ' Analytics failure should never crash the application
            End Try
        End Sub

        ''' <summary>
        ''' Records an email processing operation.
        ''' </summary>
        Public Sub TrackEmailProcessed(emailId As String, processingType As String, durationMs As Long) Implements IAnalyticsService.TrackEmailProcessed
            If Not _analyticsEnabled Then Return
            If String.IsNullOrEmpty(emailId) Then Return

            Try
                Dim today = DateTime.Today.ToString("yyyy-MM-dd")

                ' Upsert into daily Analytics table
                Dim existing = _db.ExecuteScalar(Of Integer)(
                    "SELECT COUNT(*) FROM Analytics WHERE Date = @Date",
                    Sub(cmd)
                        cmd.Parameters.AddWithValue("@Date", today)
                    End Sub
                )

                If existing > 0 Then
                    _db.Execute(
                        "UPDATE Analytics SET EmailsProcessed = EmailsProcessed + 1, " &
                        "TimeSavedMinutes = TimeSavedMinutes + CAST(@DurationMs / 60000 AS INTEGER) " &
                        "WHERE Date = @Date",
                        Sub(cmd)
                            cmd.Parameters.AddWithValue("@DurationMs", durationMs)
                            cmd.Parameters.AddWithValue("@Date", today)
                        End Sub
                    )
                Else
                    _db.Execute(
                        "INSERT INTO Analytics (Date, EmailsProcessed, TimeSavedMinutes, ResponsesGenerated, TokensConsumed) " &
                        "VALUES (@Date, 1, CAST(@DurationMs / 60000 AS INTEGER), 0, 0)",
                        Sub(cmd)
                            cmd.Parameters.AddWithValue("@DurationMs", durationMs)
                            cmd.Parameters.AddWithValue("@Date", today)
                        End Sub
                    )
                End If

                ' Also log to event log
                TrackEvent("email_processed", New Dictionary(Of String, Object) From {
                    {"email_id", emailId},
                    {"type", processingType},
                    {"duration_ms", durationMs}
                })
            Catch
                ' Analytics failure should never crash the application
            End Try
        End Sub

        ''' <summary>
        ''' Records estimated time saved for the user.
        ''' </summary>
        Public Sub TrackTimeSaved(secondsSaved As Double) Implements IAnalyticsService.TrackTimeSaved
            If Not _analyticsEnabled Then Return
            If secondsSaved <= 0 Then Return

            Try
                Dim today = DateTime.Today.ToString("yyyy-MM-dd")
                Dim minutesSaved = CInt(Math.Ceiling(secondsSaved / 60.0))

                _db.Execute(
                    "UPDATE Analytics SET TimeSavedMinutes = TimeSavedMinutes + @Minutes WHERE Date = @Date",
                    Sub(cmd)
                        cmd.Parameters.AddWithValue("@Minutes", minutesSaved)
                        cmd.Parameters.AddWithValue("@Date", today)
                    End Sub
                )
            Catch
                ' Analytics failure should never crash the application
            End Try
        End Sub

        ''' <summary>
        ''' Returns aggregate analytics for the current period.
        ''' </summary>
        Public Function GetAnalyticsSummary() As AnalyticsSummary Implements IAnalyticsService.GetAnalyticsSummary
            Dim summary = New AnalyticsSummary()

            Try
                Dim today = DateTime.Today.ToString("yyyy-MM-dd")
                Dim weekStart = DateTime.Today.AddDays(-7).ToString("yyyy-MM-dd")
                Dim monthStart = DateTime.Today.AddMonths(-1).ToString("yyyy-MM-dd")

                summary.EmailsToday = _db.ExecuteScalar(Of Integer)(
                    "SELECT COALESCE(SUM(EmailsProcessed), 0) FROM Analytics WHERE Date = @Date",
                    Sub(cmd)
                        cmd.Parameters.AddWithValue("@Date", today)
                    End Sub
                )

                summary.EmailsThisWeek = _db.ExecuteScalar(Of Integer)(
                    "SELECT COALESCE(SUM(EmailsProcessed), 0) FROM Analytics WHERE Date >= @Date",
                    Sub(cmd)
                        cmd.Parameters.AddWithValue("@Date", weekStart)
                    End Sub
                )

                summary.EmailsThisMonth = _db.ExecuteScalar(Of Integer)(
                    "SELECT COALESCE(SUM(EmailsProcessed), 0) FROM Analytics WHERE Date >= @Date",
                    Sub(cmd)
                        cmd.Parameters.AddWithValue("@Date", monthStart)
                    End Sub
                )

                summary.TotalEmailsProcessed = _db.ExecuteScalar(Of Integer)(
                    "SELECT COALESCE(SUM(EmailsProcessed), 0) FROM Analytics"
                )

                summary.TotalTimeSavedSeconds = _db.ExecuteScalar(Of Double)(
                    "SELECT COALESCE(SUM(TimeSavedMinutes), 0) * 60.0 FROM Analytics"
                )

                ' Get popular features from event log
                Dim features = New Dictionary(Of String, Integer)()
                Try
                    Dim reader = _db.ExecuteReader(
                        "SELECT EventName, COUNT(*) AS Cnt FROM EventLog " &
                        "WHERE CreatedDate >= @MonthStart GROUP BY EventName ORDER BY Cnt DESC LIMIT 10",
                        Sub(cmd)
                            cmd.Parameters.AddWithValue("@MonthStart", monthStart)
                        End Sub
                    )
                    While reader.Read()
                        Dim name = If(reader("EventName") IsNot DBNull.Value, reader("EventName").ToString(), "unknown")
                        Dim cnt = If(reader("Cnt") IsNot DBNull.Value, Convert.ToInt32(reader("Cnt")), 0)
                        features(name) = cnt
                    End While
                Catch
                End Try
                summary.PopularFeatures = features

            Catch
                ' Analytics failure should never crash the application
            End Try

            Return summary
        End Function

        ''' <summary>
        ''' Flushes any pending analytics data to storage.
        ''' In this implementation, data is written synchronously, so Flush is a no-op.
        ''' </summary>
        Public Sub Flush() Implements IAnalyticsService.Flush
            ' Data is written synchronously; no buffering needed
        End Sub

        ''' <summary>
        ''' Records token consumption for a specific provider.
        ''' </summary>
        Public Sub TrackTokensConsumed(providerName As String, tokens As Integer)
            If Not _analyticsEnabled Then Return
            If String.IsNullOrEmpty(providerName) OrElse tokens <= 0 Then Return

            Try
                Dim today = DateTime.Today.ToString("yyyy-MM-dd")

                _db.Execute(
                    "UPDATE Analytics SET TokensConsumed = TokensConsumed + @Tokens, " &
                    "ProviderName = COALESCE(ProviderName, @Provider) " &
                    "WHERE Date = @Date",
                    Sub(cmd)
                        cmd.Parameters.AddWithValue("@Tokens", tokens)
                        cmd.Parameters.AddWithValue("@Provider", providerName)
                        cmd.Parameters.AddWithValue("@Date", today)
                    End Sub
                )
            Catch
                ' Analytics failure should never crash the application
            End Try
        End Sub

        ''' <summary>
        ''' Records a response generation event.
        ''' </summary>
        Public Sub TrackResponseGenerated()
            If Not _analyticsEnabled Then Return

            Try
                Dim today = DateTime.Today.ToString("yyyy-MM-dd")

                _db.Execute(
                    "UPDATE Analytics SET ResponsesGenerated = ResponsesGenerated + 1 WHERE Date = @Date",
                    Sub(cmd)
                        cmd.Parameters.AddWithValue("@Date", today)
                    End Sub
                )
            Catch
                ' Analytics failure should never crash the application
            End Try
        End Sub

    End Class

End Namespace