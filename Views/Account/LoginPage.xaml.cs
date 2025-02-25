using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SGSClient.ViewModels;

namespace SGSClient.Views
{
    public sealed partial class LoginPage : Page
    {
        public LoginViewModel ViewModel { get; }

        public LoginPage()
        {
            ViewModel = App.GetService<LoginViewModel>();
            InitializeComponent();
            Loaded += async (s, e) => await ViewModel.CheckUserSessionAsync();
        }

        private async void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoginAsync();
        }

        private void ButtonRegister_Click(object sender, RoutedEventArgs e)
        {
            //ViewModel.NavigateToRegister();
        }

        private void HyperlinkForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            //ViewModel.NavigateToForgotPassword();
        }
    }
}
