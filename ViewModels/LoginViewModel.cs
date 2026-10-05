using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TokenApp.Resources;
using Microsoft.Maui.Controls;

namespace TokenApp.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        [ObservableProperty]
        string username;

        [ObservableProperty]
        string password;

        [ObservableProperty]
        bool rememberMe;

        public LoginViewModel()
        {
        }

        [RelayCommand(AllowConcurrentExecutions = false)]
        async Task Login()
        {
            if (IsBusy) return;
            IsBusy = true;

            // TODO: Replace with your real authentication call
            await Task.Delay(800);

            IsBusy = false;

            // On success navigate (uncomment and update route if needed)
            // await Shell.Current.GoToAsync("/home");
        }

        [RelayCommand(AllowConcurrentExecutions =false)]
        async Task ForgotPassword()
        {
            await Shell.Current.DisplayAlert(string.Empty, AppResource.ForgotPassword, "OK");
        }
    }
}
