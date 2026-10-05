using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace TokenApp.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        string username;
        string password;
        bool isBusy;
        bool rememberMe;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Username { get => username; set { username = value; OnPropertyChanged(); } }
        public string Password { get => password; set { password = value; OnPropertyChanged(); } }
        public bool IsBusy { get => isBusy; set { isBusy = value; OnPropertyChanged(); } }
        public bool RememberMe { get => rememberMe; set { rememberMe = value; OnPropertyChanged(); } }

        public ICommand LoginCommand { get; }
        public ICommand ForgotPasswordCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new Command(async () => await ExecuteLogin());
            ForgotPasswordCommand = new Command(async () => await ExecuteForgot());
        }

        async Task ExecuteLogin()
        {
            if (IsBusy) return;
            IsBusy = true;
            // Placeholder: integrate with existing authentication service if available
            await Task.Delay(800);
            IsBusy = false;

            // On success, navigate or raise event
            // await Shell.Current.GoToAsync("/home");
        }

        async Task ExecuteForgot()
        {
            // Navigate to forgot password or show dialog
            await Shell.Current.DisplayAlert("", TokenApp.Resources.AppResource.ForgotPassword, "OK");
        }

        void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
