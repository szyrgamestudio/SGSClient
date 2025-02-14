using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SGSClient.ViewModels;

namespace SGSClient.Views;

public sealed partial class LoginPage : Page
{
    #region Fields
    public LoginViewModel ViewModel { get; }
    #endregion

    #region Constructor
    public LoginPage()
    {
        ViewModel = App.GetService<LoginViewModel>();
        InitializeComponent();
        Loaded += async (s, e) => await ViewModel.CheckUserSessionAsync();
    }
    #endregion

    #region Event Handlers
    private async void ButtonLogin_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.Email = textBoxEmail.Text;
        ViewModel.Password = passwordBox1.Password;
        await ViewModel.LoginAsync();
        errormessage.Text = ViewModel.ErrorMessage;
    }
    private void ButtonRegister_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.NavigateToRegister();
    }
    private void HyperlinkForgotPassword_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.NavigateToForgotPassword();
    }
    #endregion
}
