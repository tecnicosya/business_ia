Imports System.Collections.Concurrent
Imports OutlookAIAssistant.Core.Interfaces

Namespace OutlookAIAssistant.AIEngine.Cache

    ''' <summary>
    ''' In-memory cache for AI responses and email data.
    ''' Reduces redundant API calls for repeated or similar content.
    ''' </summary>
    Public Class EmailCache
        Implements ICacheService

        Private ReadOnly _cache As New ConcurrentDictionary(Of String, CacheEntry)
        Private ReadOnly _cleanupTimer As Timer

        Private Class CacheEntry
            Public Property Value As Object
            Public Property Expiration As DateTime
            Public Property Created As DateTime = DateTime.UtcNow
        End Class

        Public Sub New()
            ' Periodic cleanup every 5 minutes
            _cleanupTimer = New Timer(AddressOf CleanupExpired, Nothing, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5))
        End Sub

        Public Function TryGetValue(Of T)(key As String) As T Implements ICacheService.TryGetValue
            If _cache.TryGetValue(key, Dim entry) AndAlso entry.Expiration > DateTime.UtcNow Then
                Return CType(entry.Value, T)
            End If
            ' Clean up expired entry
            If _cache.ContainsKey(key) Then
                _cache.TryRemove(key, Nothing)
            End If
            Return Nothing
        End Function

        Public Sub SetValue(Of T)(key As String, value As T, expiration As TimeSpan) Implements ICacheService.SetValue
            Dim entry = New CacheEntry With {
                .Value = value,
                .Expiration = DateTime.UtcNow.Add(expiration)
            }
            _cache(key) = entry
        End Sub

        Public Sub Remove(key As String) Implements ICacheService.Remove
            _cache.TryRemove(key, Nothing)
        End Sub

        Public Sub Clear() Implements ICacheService.Clear
            _cache.Clear()
        End Sub

        Public Function Contains(key As String) As Boolean Implements ICacheService.Contains
            Return _cache.ContainsKey(key) AndAlso
                _cache.TryGetValue(key, Dim entry) AndAlso
                entry.Expiration > DateTime.UtcNow
        End Function

        Private Sub CleanupExpired(state As Object)
            Dim now = DateTime.UtcNow
            For Each kvp In _cache
                If kvp.Value.Expiration <= now Then
                    _cache.TryRemove(kvp.Key, Nothing)
                End If
            Next
        End Sub

        Protected Overrides Sub Finalize()
            _cleanupTimer?.Dispose()
            MyBase.Finalize()
        End Sub
    End Class

End Namespace