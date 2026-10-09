using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TokenApp.Resources;
using Microsoft.Maui.Controls;
using TokenApp.Services.Api;
using TokenApp.Constants;
using TokenApp.Services.Auth;

namespace TokenApp.ViewModels
{
    public partial class LoginViewModel(ITokenApiService apiService,
        ISessionService sessionService) : BaseViewModel
    {
        [ObservableProperty]
        string? username;

        [ObservableProperty]
        string? password;

        [ObservableProperty]
        bool rememberMe;


        [RelayCommand(AllowConcurrentExecutions = false)]
        async Task Login()
        {

            if (IsBusy) return;
            IsBusy = true;

            var result = await apiService.LoginAsync(Username, Password);
            if (result.IsSuccess && result.Data is not null)
            {
                await sessionService.StartSessionAsync(result.Data.Token);
                await Shell.Current.GoToAsync($"//TokenHistory");
            }

            IsBusy = false;

            // On success navigate (uncomment and update route if needed)
            // await Shell.Current.GoToAsync("/home");
        }

        [RelayCommand(AllowConcurrentExecutions = false)]
        async Task ForgotPassword()
        {
            await Shell.Current.DisplayAlertAsync(string.Empty, AppResource.ForgotPassword, "OK");
        }
    }
}
