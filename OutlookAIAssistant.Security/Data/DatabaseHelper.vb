Imports System.Data.SQLite
Imports System.IO

Namespace OutlookAIAssistant.Security.Data

    ''' <summary>
    ''' Manages SQLite database operations for the Outlook AI Assistant.
    ''' Creates and initializes the local database schema on first use.
    ''' Provides helper methods for executing queries and commands.
    ''' The database is stored in the user's AppData folder.
    ''' </summary>
    Public Class DatabaseHelper

        Private Shared ReadOnly _dbPath As String
        Private Shared ReadOnly _connectionString As String
        Private Shared _initialized As Boolean = False
        Private Shared _lock As New Object()

        Shared Sub New()
            Dim appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            Dim dbDir = Path.Combine(appData, "OutlookAIAssistant")
            Directory.CreateDirectory(dbDir)
            _dbPath = Path.Combine(dbDir, "outlook_ai_assistant.db")
            _connectionString = $"Data Source={_dbPath};Version=3;Pooling=True;Max Pool Size=100;"
        End Sub

        ''' <summary>
        ''' Returns the full path to the SQLite database file.
        ''' </summary>
        Public Shared ReadOnly Property DatabasePath As String
            Get
                Return _dbPath
            End Get
        End Property

        ''' <summary>
        ''' Initializes the database schema if not already created.
        ''' Called automatically on first query.
        ''' </summary>
        Public Sub InitializeSchema()
            If _initialized Then Return

            SyncLock _lock
                If _initialized Then Return

                Using conn = New SQLiteConnection(_connectionString)
                    conn.Open()

                    Using cmd = conn.CreateCommand()
                        cmd.CommandText = GetCreateSchemaSql()
                        cmd.ExecuteNonQuery()
                    End Using
                End Using

                _initialized = True
            End SyncLock
        End Sub

        ''' <summary>
        ''' Executes a non-query command (INSERT, UPDATE, DELETE, CREATE).
        ''' </summary>
        Public Sub Execute(sql As String, Optional configureCmd As Action(Of SQLiteCommand) = Nothing)
            InitializeSchema()

            Using conn = New SQLiteConnection(_connectionString)
                conn.Open()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = sql
                    configureCmd?.Invoke(cmd)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        End Sub

        ''' <summary>
        ''' Executes a scalar query and returns the result.
        ''' </summary>
        Public Function ExecuteScalar(Of T)(sql As String, Optional configureCmd As Action(Of SQLiteCommand) = Nothing) As T
            InitializeSchema()

            Using conn = New SQLiteConnection(_connectionString)
                conn.Open()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = sql
                    configureCmd?.Invoke(cmd)
                    Dim result = cmd.ExecuteScalar()
                    If result Is Nothing OrElse result Is DBNull.Value Then
                        Return Nothing
                    End If
                    Return CType(Convert.ChangeType(result, GetType(T)), T)
                End Using
            End Using
        End Function

        ''' <summary>
        ''' Executes a query and returns a SQLiteDataReader for reading results.
        ''' The caller must dispose the reader.
        ''' </summary>
        Public Function ExecuteReader(sql As String, Optional configureCmd As Action(Of SQLiteCommand) = Nothing) As SQLiteDataReader
            InitializeSchema()

            Dim conn = New SQLiteConnection(_connectionString)
            conn.Open()
            Dim cmd = conn.CreateCommand()
            cmd.CommandText = sql
            configureCmd?.Invoke(cmd)
            Return cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection)
        End Function

        ''' <summary>
        ''' Returns the SQL for creating all application tables.
        ''' </summary>
        Private Shared Function GetCreateSchemaSql() As String
            Return "
                CREATE TABLE IF NOT EXISTS Configuration (
                    Key TEXT PRIMARY KEY,
                    Value TEXT NOT NULL,
                    Encrypted INTEGER DEFAULT 0
                );

                CREATE TABLE IF NOT EXISTS License (
                    Id INTEGER PRIMARY KEY,
                    LicenseKey TEXT,
                    Plan TEXT NOT NULL,
                    ActivatedDate TEXT,
                    ExpiryDate TEXT,
                    LastCheck TEXT,
                    OfflineDays INTEGER DEFAULT 0
                );

                CREATE TABLE IF NOT EXISTS Analytics (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Date TEXT NOT NULL,
                    EmailsProcessed INTEGER DEFAULT 0,
                    TimeSavedMinutes INTEGER DEFAULT 0,
                    ResponsesGenerated INTEGER DEFAULT 0,
                    TokensConsumed INTEGER DEFAULT 0,
                    ProviderName TEXT
                );

                CREATE TABLE IF NOT EXISTS Rules (
                    Id TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    RuleJson TEXT NOT NULL,
                    Enabled INTEGER DEFAULT 1,
                    Priority INTEGER DEFAULT 0,
                    CreatedDate TEXT,
                    ModifiedDate TEXT
                );

                CREATE TABLE IF NOT EXISTS Cache (
                    EmailHash TEXT PRIMARY KEY,
                    Response TEXT NOT NULL,
                    CreatedDate TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS EventLog (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    EventName TEXT NOT NULL,
                    Properties TEXT,
                    CreatedDate TEXT NOT NULL
                );

                CREATE INDEX IF NOT EXISTS idx_analytics_date ON Analytics(Date);
                CREATE INDEX IF NOT EXISTS idx_eventlog_name ON EventLog(EventName);
                CREATE INDEX IF NOT EXISTS idx_cache_date ON Cache(CreatedDate);
            "
        End Function

    End Class

End Namespace