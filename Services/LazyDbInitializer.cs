using TokenApp.Services.Database;

namespace TokenApp.Services
{
    /// <summary>
    /// Lazy initializer for SQLite database
    /// Starts database initialization asynchronously at app startup
    /// </summary>
    public class LazyDbInitializer
    {
        private readonly AppDbContext _dbContext;

        public LazyDbInitializer(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

            // Start initialization asynchronously (fire and forget)
            // This will ensure the database is ready when needed
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await _dbContext.InitializeAsync();
                    System.Diagnostics.Debug.WriteLine("Database initialized successfully.");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
                }
            });
        }
    }
}
