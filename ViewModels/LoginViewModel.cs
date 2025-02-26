using CommunityToolkit.Mvvm.ComponentModel;
using SGSClient.Contracts.Services;
using SGSClient.Core.Authorization;
using WinRT.Interop;

namespace SGSClient.ViewModels
{
    public partial class LoginViewModel : ObservableRecipient
    {
        private readonly INavigationService _navigationService;
        private readonly IAppUser _appUser;

        public LoginViewModel(INavigationService navigationService, IAppUser appUser)
        {
            _navigationService = navigationService;
            _appUser = appUser;
        }

        public async Task CheckUserSessionAsync()
        {
            if (_appUser.IsLoggedIn)
            {
                _navigationService.NavigateTo(typeof(MyAccountViewModel).FullName!);
            }
        }

        public async Task LoginAsync()
        {
            IntPtr hwnd = WindowNative.GetWindowHandle(App.MainWindow);
            bool loginSuccess = await _appUser.LoginAsync(hwnd, false);

            if (loginSuccess)
            {
                _navigationService.NavigateTo(typeof(HomeViewModel).FullName!);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Błąd logowania przez Entra ID.");
            }
        }

        public async Task LogoutAsync()
        {
            IntPtr hwnd = WindowNative.GetWindowHandle(App.MainWindow);
            await _appUser.LogoutAsync(hwnd);
            System.Diagnostics.Debug.WriteLine("Wylogowano użytkownika.");
        }
    }
}
