using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TokenApp.Models;
using TokenApp.Services.Api;
using TokenApp.Services.Data;

namespace TokenApp.ViewModels
{
    /// <summary>
    /// Token history shown from the backend dashboard response: summary counts, today's token
    /// numbers and the token list grouped by date, with search and type filtering done locally.
    /// </summary>
    public partial class TokenHistoryViewModel : BaseViewModel
    {
        private const string LoadFailedMessage = "Could not load tokens.";

        private readonly ITokenApiService _api;
        private readonly IAuthRepository _auth;
        private readonly ITokenRepository _tokenRepository;

        private List<TokenDTO> _allTokens = new();
        private int _isLoading;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasSummary))]
        DashboardSummary? summary;

        [ObservableProperty]
        ObservableCollection<TokenGroup> tokenGroups = new();

        [ObservableProperty]
        string searchQuery = string.Empty;

        [ObservableProperty]
        string resultText = string.Empty;

        [ObservableProperty]
        string emptyMessage = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasError))]
        string? errorMessage;

        [ObservableProperty]
        bool isRefreshing;

        public bool HasSummary => Summary != null;
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public ObservableCollection<TokenFilterOption> FilterOptions { get; }

        public TokenHistoryViewModel(ITokenApiService api, IAuthRepository auth, ITokenRepository tokenRepository)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _auth = auth ?? throw new ArgumentNullException(nameof(auth));
            _tokenRepository = tokenRepository ?? throw new ArgumentNullException(nameof(tokenRepository));
            Title = "Token History";

            FilterOptions = new ObservableCollection<TokenFilterOption>
            {
                new("All", null, SelectFilterCommand) { IsSelected = true },
                new("Yellow", TokenType.Yellow, SelectFilterCommand),
                new("Red", TokenType.Red, SelectFilterCommand),
                new("Emergency", TokenType.Emergency, SelectFilterCommand)
            };
        }

        /// <summary>Called when the page appears: spinner only while there is nothing to show yet.</summary>
        [RelayCommand]
        private async Task LoadAsync()
        {
            if (HasSummary)
            {
                await FetchAsync();
                return;
            }

            IsBusy = true;
            try
            {
                await FetchAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>Pull-to-refresh and the error banner's Retry.</summary>
        [RelayCommand]
        private async Task RefreshAsync()
        {
            IsRefreshing = true;
            try
            {
                await FetchAsync();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private void SelectFilter(TokenFilterOption? option)
        {
            if (option == null)
                return;

            foreach (var item in FilterOptions)
                item.IsSelected = item == option;

            ApplyFilters();
        }

        [RelayCommand(AllowConcurrentExecutions = false)]
        private async Task LogoutAsync()
        {
            var message = "You will need to sign in again to continue.";

            var unsynced = await GetUnsyncedCountAsync();
            if (unsynced > 0)
            {
                var noun = unsynced == 1 ? "token has" : "tokens have";
                message = $"{unsynced} {noun} not been sent to the server yet. They will be sent the next time you sign in.";
            }

            var confirmed = await Shell.Current.DisplayAlertAsync("Log out?", message, "Log out", "Cancel");
            if (!confirmed)
                return;

            await _auth.LogoutAsync();
            ResetState();
            await Shell.Current.GoToAsync("//MainPage");
        }

        partial void OnSearchQueryChanged(string value) => ApplyFilters();

        private async Task FetchAsync()
        {
            // Appearing and pull-to-refresh can overlap; one request at a time is enough.
            if (Interlocked.Exchange(ref _isLoading, 1) == 1)
                return;

            try
            {
                var result = await _api.GetDashboardAsync();
                if (result.IsSuccess && result.Data != null)
                {
                    ErrorMessage = null;
                    Summary = result.Data;
                    _allTokens = result.Data.Tokens;
                }
                else
                {
                    // Keep whatever is already on screen; just explain why it may be stale.
                    ErrorMessage = string.IsNullOrWhiteSpace(result.Message) ? LoadFailedMessage : result.Message;
                }

                ApplyFilters();
            }
            finally
            {
                Interlocked.Exchange(ref _isLoading, 0);
            }
        }

        private void ApplyFilters()
        {
            var selectedType = FilterOptions.FirstOrDefault(o => o.IsSelected)?.Type;
            var query = SearchQuery?.Trim() ?? string.Empty;

            var matches = _allTokens.Where(t =>
                (selectedType == null || t.Type == selectedType) &&
                (query.Length == 0 || Matches(t, query)));

            var groups = matches
                .GroupBy(t => t.DateTime.Date)
                .OrderByDescending(g => g.Key)
                .Select(g => new TokenGroup(
                    g.Key,
                    g.OrderByDescending(t => t.CreatedAt ?? t.DateTime).ThenByDescending(t => t.RemoteId)))
                .ToList();

            TokenGroups = new ObservableCollection<TokenGroup>(groups);

            var count = groups.Sum(g => g.Count);
            ResultText = count == 1 ? "1 token" : $"{count} tokens";

            EmptyMessage = !HasSummary
                ? LoadFailedMessage
                : _allTokens.Count == 0 ? "No tokens yet." : "No tokens match your search or filter.";
        }

        private static bool Matches(TokenDTO token, string query)
        {
            const StringComparison ignoreCase = StringComparison.OrdinalIgnoreCase;
            var applicant = token.Applicant;

            return token.TokenNumber.ToString(CultureInfo.InvariantCulture).Contains(query, ignoreCase)
                || token.DateTime.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture).Contains(query, ignoreCase)
                || token.DateTime.ToString("dd MMM yyyy", CultureInfo.CurrentCulture).Contains(query, ignoreCase)
                || (applicant != null
                    && (applicant.Name.Contains(query, ignoreCase)
                        || applicant.Mobile.Contains(query, ignoreCase)
                        || applicant.City.Contains(query, ignoreCase)));
        }

        private async Task<int> GetUnsyncedCountAsync()
        {
            try
            {
                return await _tokenRepository.GetPendingSyncCountAsync();
            }
            catch (Exception ex)
            {
                // Only used to enrich the confirmation text; never block logging out.
                System.Diagnostics.Debug.WriteLine($"Pending sync count failed: {ex.GetType().Name}");
                return 0;
            }
        }

        /// <summary>Drops the signed-out admin's data so the next sign-in never flashes it.</summary>
        private void ResetState()
        {
            _allTokens = new List<TokenDTO>();
            Summary = null;
            ErrorMessage = null;
            SearchQuery = string.Empty;
            foreach (var item in FilterOptions)
                item.IsSelected = item.Type == null;

            ApplyFilters();
        }
    }
}
