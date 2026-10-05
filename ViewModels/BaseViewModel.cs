using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TokenApp.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        string title;

        [ObservableProperty]
        bool isBusy;

        /// <summary>
        /// Helper to run asynchronous work with IsBusy toggled and basic error handling.
        /// </summary>
        protected async Task RunBusyAsync(Func<Task> work)
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                if (work != null)
                    await work();
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
