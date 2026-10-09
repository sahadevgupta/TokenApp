namespace TokenApp.Services.Auth
{
    /// <summary>
    /// Holds the Sanctum bearer token. The token lives only in SecureStorage and is never logged.
    /// </summary>
    public interface ISessionService
    {
        Task<string?> GetTokenAsync();
        Task<bool> HasSessionAsync();
        Task StartSessionAsync(string token);

        /// <summary>Ends the session on user request (logout). Does not raise <see cref="SessionExpired"/>.</summary>
        Task ClearAsync();

        /// <summary>Called when the API answers 401: clears the token and raises <see cref="SessionExpired"/>.</summary>
        Task ExpireAsync();

        event EventHandler? SessionStarted;
        event EventHandler? SessionExpired;
    }

    public class SessionService : ISessionService
    {
        private const string TokenKey = "auth_token";

        private readonly SemaphoreSlim _gate = new(1, 1);
        private string? _cachedToken;
        private bool _loaded;

        public event EventHandler? SessionStarted;
        public event EventHandler? SessionExpired;

        public async Task<string?> GetTokenAsync()
        {
            await _gate.WaitAsync();
            try
            {
                await EnsureLoadedAsync();
                return _cachedToken;
            }
            finally
            {
                _gate.Release();
            }
        }

        // Caller must hold _gate.
        private async Task EnsureLoadedAsync()
        {
            if (_loaded)
                return;

            try
            {
                _cachedToken = await SecureStorage.Default.GetAsync(TokenKey);
            }
            catch (Exception ex)
            {
                // SecureStorage can throw when the keystore is unavailable/corrupt: treat as signed out.
                System.Diagnostics.Debug.WriteLine($"SecureStorage read failed: {ex.GetType().Name}");
                _cachedToken = null;
            }
            _loaded = true;
        }

        public async Task<bool> HasSessionAsync() => !string.IsNullOrEmpty(await GetTokenAsync());

        public async Task StartSessionAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token is required.", nameof(token));

            await _gate.WaitAsync();
            try
            {
                await SecureStorage.Default.SetAsync(TokenKey, token);
                _cachedToken = token;
                _loaded = true;
            }
            finally
            {
                _gate.Release();
            }

            SessionStarted?.Invoke(this, EventArgs.Empty);
        }

        public async Task ClearAsync()
        {
            await _gate.WaitAsync();
            try
            {
                SecureStorage.Default.Remove(TokenKey);
                _cachedToken = null;
                _loaded = true;
            }
            finally
            {
                _gate.Release();
            }
        }

        public async Task ExpireAsync()
        {
            // Several in-flight requests can all see 401; only the first one that actually
            // held a token should notify the UI.
            bool hadToken;
            await _gate.WaitAsync();
            try
            {
                await EnsureLoadedAsync();
                hadToken = !string.IsNullOrEmpty(_cachedToken);
                SecureStorage.Default.Remove(TokenKey);
                _cachedToken = null;
                _loaded = true;
            }
            finally
            {
                _gate.Release();
            }

            if (hadToken)
                SessionExpired?.Invoke(this, EventArgs.Empty);
        }
    }
}
