using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TokenApp.Models;
using TokenApp.Services.Data;

namespace TokenApp.ViewModels
{
    public partial class TokenHistoryViewModel : BaseViewModel
    {
        private readonly ITokenRepository _tokenRepository;

        [ObservableProperty]
        ObservableCollection<TokenDTO> tokens = new();

        [ObservableProperty]
        ObservableCollection<TokenDTO> filteredTokens = new();

        [ObservableProperty]
        string searchQuery = string.Empty;

        [ObservableProperty]
        DateTime selectedDate = DateTime.Today;

        [ObservableProperty]
        TokenType? selectedTokenType = null;

        [ObservableProperty]
        bool isSearching = false;

        [ObservableProperty]
        int totalTokenCount = 0;

        [ObservableProperty]
        int yellowTokenCount = 0;

        [ObservableProperty]
        int redTokenCount = 0;

        [ObservableProperty]
        int emergencyTokenCount = 0;

        public TokenHistoryViewModel(ITokenRepository tokenRepository)
        {
            _tokenRepository = tokenRepository ?? throw new ArgumentNullException(nameof(tokenRepository));
            Title = "History/Queue";
        }

        [RelayCommand]
        public async Task LoadTokensAsync()
        {
            await RunBusyAsync(async () =>
            {
                try
                {
                    var tokens = await _tokenRepository.GetAllTokensAsync();

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Tokens.Clear();
                        foreach (var token in tokens.OrderByDescending(t => t.DateTime))
                        {
                            Tokens.Add(token);
                        }
                    });

                    // Load token counts
                    await UpdateTokenCountsAsync();
                    ApplyFilters();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading tokens: {ex.Message}");
                }
            });
        }

        [RelayCommand]
        public void SearchTokens()
        {
            ApplyFilters();
        }

        [RelayCommand]
        public void ClearFilters()
        {
            SelectedTokenType = null;
            ApplyFilters();
        }

        [RelayCommand]
        public void FilterAll()
        {
            SelectedTokenType = null;
            ApplyFilters();
        }

        [RelayCommand]
        public void FilterYellow()
        {
            SelectedTokenType = TokenType.Yellow;
            ApplyFilters();
        }

        [RelayCommand]
        public void FilterRed()
        {
            SelectedTokenType = TokenType.Red;
            ApplyFilters();
        }

        [RelayCommand]
        public void FilterEmergency()
        {
            SelectedTokenType = TokenType.Emergency;
            ApplyFilters();
        }

        [RelayCommand]
        public async Task ExportTokensAsync()
        {
            await RunBusyAsync(async () =>
            {
                try
                {
                    // TODO: Implement CSV export functionality
                    // Export FilteredTokens to CSV
                    await Task.Delay(500); // Simulate work
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error exporting tokens: {ex.Message}");
                }
            });
        }

        [RelayCommand]
        public async Task PrintSummaryAsync()
        {
            await RunBusyAsync(async () =>
            {
                try
                {
                    // TODO: Implement print functionality
                    await Task.Delay(500); // Simulate work
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error printing summary: {ex.Message}");
                }
            });
        }

        [RelayCommand]
        public async Task ReprintTokenAsync(TokenDTO token)
        {
            await RunBusyAsync(async () =>
            {
                try
                {
                    // TODO: Implement reprint functionality
                    // Call printer service
                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error reprinting token: {ex.Message}");
                }
            });
        }

        [RelayCommand]
        public async Task ShowTokenDetailsAsync(TokenDTO token)
        {
            await RunBusyAsync(async () =>
            {
                try
                {
                    // TODO: Navigate to token details page
                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error showing token details: {ex.Message}");
                }
            });
        }

        [RelayCommand]
        public async Task EscortTokenAsync(TokenDTO token)
        {
            await RunBusyAsync(async () =>
            {
                try
                {
                    // TODO: Implement escort functionality
                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error escorting token: {ex.Message}");
                }
            });
        }

        private void ApplyFilters()
        {
            var query = Tokens.AsEnumerable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var searchLower = SearchQuery.ToLower();
                query = query.Where(t =>
                    t.TokenNumber.ToString().Contains(SearchQuery) ||
                    (!string.IsNullOrEmpty(t.Location) && t.Location.ToLower().Contains(searchLower)) ||
                    (!string.IsNullOrEmpty(t.BatchNumber) && t.BatchNumber.ToLower().Contains(searchLower)) ||
                    (!string.IsNullOrEmpty(t.Escort) && t.Escort.ToLower().Contains(searchLower)));
            }

            // Apply token type filter
            if (SelectedTokenType.HasValue)
            {
                query = query.Where(t => t.Type == SelectedTokenType.Value);
            }

            // Apply date filter
            query = query.Where(t => t.DateTime.Date == SelectedDate.Date);

            var filtered = query.OrderByDescending(t => t.DateTime).ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                FilteredTokens.Clear();
                foreach (var token in filtered)
                {
                    FilteredTokens.Add(token);
                }
            });
        }

        private async Task UpdateTokenCountsAsync()
        {
            try
            {
                TotalTokenCount = await _tokenRepository.GetTokenCountAsync();
                YellowTokenCount = await _tokenRepository.GetTokenCountByTypeAsync(TokenType.Yellow);
                RedTokenCount = await _tokenRepository.GetTokenCountByTypeAsync(TokenType.Red);
                EmergencyTokenCount = await _tokenRepository.GetTokenCountByTypeAsync(TokenType.Emergency);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating token counts: {ex.Message}");
            }
        }
    }
}
