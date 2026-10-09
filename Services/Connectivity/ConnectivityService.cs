namespace TokenApp.Services.Connectivity
{
    public interface IConnectivityService
    {
        bool IsOnline { get; }

        /// <summary>Raised with the new state whenever connectivity flips between online and offline.</summary>
        event EventHandler<bool>? OnlineChanged;
    }

    /// <summary>Thin wrapper over MAUI Essentials <see cref="IConnectivity"/> so it can be injected and mocked.</summary>
    public class ConnectivityService : IConnectivityService
    {
        private readonly IConnectivity _connectivity;
        private bool _lastOnline;

        public event EventHandler<bool>? OnlineChanged;

        public ConnectivityService(IConnectivity connectivity)
        {
            _connectivity = connectivity ?? throw new ArgumentNullException(nameof(connectivity));
            _lastOnline = IsOnline;
            _connectivity.ConnectivityChanged += OnConnectivityChanged;
        }

        public bool IsOnline => _connectivity.NetworkAccess == NetworkAccess.Internet;

        private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            var online = IsOnline;
            if (online == _lastOnline)
                return;

            _lastOnline = online;
            OnlineChanged?.Invoke(this, online);
        }
    }
}
