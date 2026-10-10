using TokenApp.ViewModels;

namespace TokenApp.Views;

public partial class TokenHistoryPage : ContentPage
{
    private readonly TokenHistoryViewModel _viewModel;

    public TokenHistoryPage(TokenHistoryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Fetch the latest dashboard whenever the page is shown (e.g. after signing in).
        if (_viewModel.LoadCommand.CanExecute(null))
        {
            await _viewModel.LoadCommand.ExecuteAsync(null);
        }
    }
}
