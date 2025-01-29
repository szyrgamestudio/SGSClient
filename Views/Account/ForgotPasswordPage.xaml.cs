using System.Net.Mail;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SGSClient.Core.Interface;
using SGSClient.Services;
using SGSClient.ViewModels;

namespace SGSClient.Views;
public sealed partial class ForgotPasswordPage : Page
{
    #region Properties
    public ForgotPasswordViewModel ViewModel
    {
        get;
    }
    #endregion

    #region Constructors
    public ForgotPasswordPage()
    {
        ViewModel = App.GetService<ForgotPasswordViewModel>();
        InitializeComponent();
    }
    #endregion

    #region Event Handlers
    private void BtnSendResetCodePassword_Click(object sender, RoutedEventArgs e)
    {
        string email = textBoxEmail.Text;
        if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
        {
            errorMessage.Message = "Wprowadź poprawny adres e-mail";
            errorMessage.Visibility = Visibility.Visible;
            errorMessage.IsOpen = true;
            return;
        }
        else
        {
            ViewModel.SendResetCode(email);
        }
    }
    private async void buttonResetPassword_Click(object sender, RoutedEventArgs e)
    {
        string email = textBoxEmail.Text;
        string token = textBoxAccessKey.Text;

        await ViewModel.TokenValidation(email, token);

        if (ViewModel.IsTokenValid)
        {
            labelNewPassword.Visibility = Visibility.Visible;
            textBoxNewPassword.Visibility = Visibility.Visible;
            labelRepeatPassword.Visibility = Visibility.Visible;
            textBoxRepeatPassword.Visibility = Visibility.Visible;
            buttonSaveNewPassword.Visibility = Visibility.Visible;
        }
        else if (!string.IsNullOrEmpty(ViewModel.ErrorMessage))
        {
            errorMessage.Message = ViewModel.ErrorMessage;
            errorMessage.Visibility = Visibility.Visible;
            errorMessage.IsOpen = true;
        }
    }
    private async void buttonSaveNewPassword_Click(object sender, RoutedEventArgs e)
    {
        string email = textBoxEmail.Text;
        string newPassword = textBoxNewPassword.Password;
        string repeatPassword = textBoxRepeatPassword.Password;

        if (newPassword == repeatPassword)
        {
            IPasswordHasher passwordHasher = new PasswordHasher();
            string securePassword = passwordHasher.HashPassword(newPassword);

            await ViewModel.UpdatePasswordInDatabase(email, securePassword);
            Frame.Navigate(typeof(LoginPage), new DrillInNavigationTransitionInfo());
        }
        else
        {
            errorMessage.Message = "Hasła się nie zgadzają.";
            errorMessage.Visibility = Visibility.Visible;
            errorMessage.IsOpen = true;
        }
    }
    #endregion

    #region Private Methods

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    #endregion
}
