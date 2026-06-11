Imports System.Security.Cryptography
Imports System.Text

Namespace OutlookAIAssistant.Security.Encryption

    ''' <summary>
    ''' Provides encryption services using Windows DPAPI (Data Protection API).
    ''' Used for securely storing API keys and sensitive configuration.
    ''' </summary>
    Public Class DPAPIEncryption

        ''' <summary>
        ''' Encrypts a plaintext string using DPAPI (current user scope).
        ''' Returns Base64-encoded ciphertext.
        ''' </summary>
        Public Function Encrypt(plainText As String) As String
            If String.IsNullOrEmpty(plainText) Then Return String.Empty

            Dim plainBytes = Encoding.UTF8.GetBytes(plainText)
            Dim encryptedBytes = ProtectedData.Protect(plainBytes, Nothing, DataProtectionScope.CurrentUser)
            Return Convert.ToBase64String(encryptedBytes)
        End Function

        ''' <summary>
        ''' Decrypts a Base64-encoded DPAPI ciphertext back to plaintext.
        ''' </summary>
        Public Function Decrypt(cipherText As String) As String
            If String.IsNullOrEmpty(cipherText) Then Return String.Empty

            Dim cipherBytes = Convert.FromBase64String(cipherText)
            Dim plainBytes = ProtectedData.Unprotect(cipherBytes, Nothing, DataProtectionScope.CurrentUser)
            Return Encoding.UTF8.GetString(plainBytes)
        End Function

        ''' <summary>
        ''' Encrypts and stores a value in the registry or configuration store.
        ''' </summary>
        Public Sub StoreSecureValue(key As String, value As String)
            Dim encrypted = Encrypt(value)
            ' In production: store in Windows registry (HKCU) or encrypted config file
            System.Diagnostics.Debug.WriteLine($"Stored encrypted value for '{key}'.")
        End Sub

        ''' <summary>
        ''' Retrieves and decrypts a secure value.
        ''' </summary>
        Public Function RetrieveSecureValue(key As String) As String
            ' In production: retrieve from Windows registry or encrypted config file
            Return String.Empty
        End Function

    End Class

End Namespace