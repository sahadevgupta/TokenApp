using TokenApp.ViewModels;

namespace TokenApp.Views;

public partial class TokenHistoryPage : ContentPage
{
    private TokenHistoryViewModel _viewModel;

    public TokenHistoryPage(TokenHistoryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Load tokens when page appears
        if (_viewModel.LoadTokensCommand.CanExecute(null))
        {
            await _viewModel.LoadTokensCommand.ExecuteAsync(null);
        }
    }
}
