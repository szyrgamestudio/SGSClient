using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SGSClient.ViewModels;

namespace SGSClient.Views
{
    public sealed partial class RegisterPage : Page
    {
        public RegisterViewModel ViewModel { get; }
        public RegisterPage()
        {
            ViewModel = App.GetService<RegisterViewModel>();
            InitializeComponent();
        }
        private async void ButtonRegister_Click(object sender, RoutedEventArgs e)
        {
            string email = textBoxEmail.Text;
            string username = textBoxAccountName.Text;
            string password = passwordBox1.Password;

            if (string.IsNullOrEmpty(passwordBox1.Password))
                errormessage.Text = "Podaj hasło.";
            else if (string.IsNullOrEmpty(passwordBoxConfirm.Password))
                errormessage.Text = "Potwierdź hasło.";
            else if (passwordBox1.Password != passwordBoxConfirm.Password)
                errormessage.Text = "Potwierdzenie hasła musi być takie samo jak hasło.";
            else
                await ViewModel.RegisterUserAsync(email, username, password);

            errormessage.Text = ViewModel.ErrorMessage;
            if (string.IsNullOrEmpty(ViewModel.ErrorMessage))
            {
                Reset();
            }
        }
        private void Reset()
        {
            textBoxAccountName.Text = "";
            textBoxEmail.Text = "";
            passwordBox1.Password = "";
            passwordBoxConfirm.Password = "";
        }
    }
}
