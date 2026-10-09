using TokenApp.Services.Auth;
using TokenApp.Services.Connectivity;
using TokenApp.Services.Data;

namespace TokenApp.Services.Sync
{
    /// <summary>
    /// Decides WHEN to sync queued tokens; ITokenRepository.SyncPendingAsync decides HOW.
    /// Triggers: app start, app resume, connectivity restored, successful sign-in.
    /// Runs are serialized inside the repository, so overlapping triggers are harmless.
    /// </summary>
    public interface ITokenSyncCoordinator
    {
        /// <summary>Subscribes to triggers. Idempotent.</summary>
        void Start();

        /// <summary>Requests a sync pass. Never throws.</summary>
        Task RequestSyncAsync();
    }

    public class TokenSyncCoordinator : ITokenSyncCoordinator
    {
        private readonly ITokenRepository _repository;
        private readonly IConnectivityService _connectivity;
        private readonly ISessionService _session;
        private int _started;

        public TokenSyncCoordinator(ITokenRepository repository, IConnectivityService connectivity, ISessionService session)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _connectivity = connectivity ?? throw new ArgumentNullException(nameof(connectivity));
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public void Start()
        {
            if (Interlocked.Exchange(ref _started, 1) == 1)
                return;

            _connectivity.OnlineChanged += OnOnlineChanged;
            _session.SessionStarted += OnSessionStarted;
        }

        public async Task RequestSyncAsync()
        {
            try
            {
                await _repository.SyncPendingAsync();
            }
            catch (Exception ex)
            {
                // Background work: log and keep the queue intact for the next trigger.
                System.Diagnostics.Debug.WriteLine($"Token sync failed: {ex.GetType().Name}: {ex.Message}");
            }
        }

        private async void OnOnlineChanged(object? sender, bool online)
        {
            if (online)
                await RequestSyncAsync();
        }

        private async void OnSessionStarted(object? sender, EventArgs e) => await RequestSyncAsync();
    }
}
