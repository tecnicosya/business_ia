Namespace OutlookAIAssistant.Core.Interfaces

    ''' <summary>
    ''' Interface for analytics tracking — measures time saved, emails processed, feature usage.
    ''' Data used for KPIs (MRR, conversion, churn) and user productivity reports.
    ''' </summary>
    Public Interface IAnalyticsService

        ''' <summary>
        ''' Tracks an event with optional properties.
        ''' </summary>
        Sub TrackEvent(eventName As String, Optional properties As IDictionary(Of String, Object) = Nothing)

        ''' <summary>
        ''' Records an email processing operation.
        ''' </summary>
        Sub TrackEmailProcessed(emailId As String, processingType As String, durationMs As Long)

        ''' <summary>
        ''' Records estimated time saved for the user.
        ''' </summary>
        Sub TrackTimeSaved(secondsSaved As Double)

        ''' <summary>
        ''' Returns aggregate analytics for the current period.
        ''' </summary>
        Function GetAnalyticsSummary() As AnalyticsSummary

        ''' <summary>
        ''' Flushes any pending analytics data to storage.
        ''' </summary>
        Sub Flush()

    End Interface

    ''' <summary>
    ''' Summary of analytics data for reporting.
    ''' </summary>
    Public Class AnalyticsSummary
        Public Property TotalEmailsProcessed As Integer
        Public Property TotalTimeSavedSeconds As Double
        Public Property EmailsToday As Integer
        Public Property EmailsThisWeek As Integer
        Public Property EmailsThisMonth As Integer
        Public Property PopularFeatures As Dictionary(Of String, Integer)
    End Class

End Namespace