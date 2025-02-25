using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SGSClient.ViewModels;

namespace SGSClient.Views
{
    public sealed partial class RegisterPage : Page
    {
        #region Fields
        public RegisterViewModel ViewModel { get; }
        #endregion

        #region Constructor
        public RegisterPage()
        {
            ViewModel = App.GetService<RegisterViewModel>();
            InitializeComponent();
        }
        #endregion

        #region Event Handlers
        private async void ButtonRegister_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Email = textBoxEmail.Text;
            ViewModel.Username = textBoxAccountName.Text;
            ViewModel.Password = passwordBox1.Password;
            ViewModel.PasswordConfirmed = passwordBoxConfirm.Password;
            await ViewModel.RegisterAsync();
            errormessage.Text = ViewModel.ErrorMessage;
        }
        #endregion
    }
}