using SQLite;
using System;
using System.IO;
using System.Threading.Tasks;
using TokenApp.Models.Database;

namespace TokenApp.Services.Database
{
    /// <summary>
    /// SQLite database context for TokenApp
    /// Handles database initialization, connections, and migrations
    /// </summary>
    public class AppDbContext
    {
        private static AppDbContext _instance;
        private static readonly object _syncLock = new object();
        private SQLiteAsyncConnection _connection;
        private Task? _initTask;

        private const string DatabaseFilename = "tokenapp.db3";

        private string DatabasePath => Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

        public static AppDbContext Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncLock)
                    {
                        _instance ??= new AppDbContext();
                    }
                }
                return _instance;
            }
        }

        private AppDbContext()
        {
        }

        /// <summary>
        /// Initialize the database connection and create tables
        /// </summary>
        public Task InitializeAsync()
        {
            // Cache the in-flight task so concurrent callers all wait for table creation
            // instead of seeing a half-initialised connection.
            lock (_syncLock)
            {
                return _initTask ??= Task.Run(InitializeCoreAsync);
            }
        }

        private async Task InitializeCoreAsync()
        {
            try
            {
                _connection = new SQLiteAsyncConnection(DatabasePath);

                // Create all tables if they don't exist
                await CreateTablesAsync();
            }
            catch
            {
                lock (_syncLock)
                {
                    _connection = null;
                    _initTask = null;
                }
                throw;
            }
        }

        /// <summary>
        /// Get the async database connection
        /// </summary>
        public SQLiteAsyncConnection GetConnection()
        {
            if (_connection == null)
                throw new InvalidOperationException("Database not initialized. Call InitializeAsync first.");
            return _connection;
        }

        /// <summary>
        /// Create all database tables if they don't exist
        /// </summary>
        private async Task CreateTablesAsync()
        {
            await _connection.CreateTableAsync<DBToken>();

            // Add any other table creations here as the app grows
        }

        /// <summary>
        /// Clear all data (for testing/reset purposes)
        /// </summary>
        public async Task ClearAsync()
        {
            if (_connection == null)
                return;

            await _connection.DeleteAllAsync<DBToken>();
        }

        /// <summary>
        /// Close and dispose the database connection
        /// </summary>
        public async Task CloseAsync()
        {
            if (_connection != null)
            {
                await _connection.CloseAsync();
                lock (_syncLock)
                {
                    _connection = null;
                    _initTask = null;
                }
            }
        }

        /// <summary>
        /// Get database file info (for debugging)
        /// </summary>
        public FileInfo GetDatabaseFileInfo()
        {
            return new FileInfo(DatabasePath);
        }
    }
}
