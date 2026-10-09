using Microsoft.Extensions.DependencyInjection;
using TokenApp.Services.Auth;
using TokenApp.Services.Sync;

namespace TokenApp
{
    public partial class App : Application
    {
        private readonly ITokenSyncCoordinator _syncCoordinator;

        public App(ITokenSyncCoordinator syncCoordinator, ISessionService session)
        {
            InitializeComponent();

            _syncCoordinator = syncCoordinator;
            _syncCoordinator.Start();

            // API returned 401: the stored session was cleared, send the user back to Login.
            session.SessionExpired += OnSessionExpired;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());

            // Sync queued tokens on launch and whenever the app returns to the foreground.
            window.Created += async (_, _) => await _syncCoordinator.RequestSyncAsync();
            window.Resumed += async (_, _) => await _syncCoordinator.RequestSyncAsync();

            return window;
        }

        private async void OnSessionExpired(object? sender, EventArgs e)
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    if (Shell.Current != null)
                        await Shell.Current.GoToAsync("//MainPage");
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation to login failed: {ex.Message}");
            }
        }
    }
}
