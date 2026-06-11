Namespace OutlookAIAssistant.Core.Interfaces

    ''' <summary>
    ''' Interface for caching AI responses and email data to reduce redundant API calls.
    ''' </summary>
    Public Interface ICacheService

        ''' <summary>
        ''' Attempts to retrieve a cached value. Returns Nothing if not found.
        ''' </summary>
        Function TryGetValue(Of T)(key As String) As T

        ''' <summary>
        ''' Stores a value with the specified key and expiration timespan.
        ''' </summary>
        Sub SetValue(Of T)(key As String, value As T, expiration As TimeSpan)

        ''' <summary>
        ''' Removes a cached entry by key.
        ''' </summary>
        Sub Remove(key As String)

        ''' <summary>
        ''' Clears all cached entries.
        ''' </summary>
        Sub Clear()

        ''' <summary>
        ''' Returns whether a key exists in the cache.
        ''' </summary>
        Function Contains(key As String) As Boolean

    End Interface

End Namespace