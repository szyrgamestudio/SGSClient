using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using SGSClient.ViewModels;

namespace SGSClient.Views;

public sealed partial class LoginPage : Page
{
    public LoginViewModel ViewModel
    {
        get;
    }

    public LoginPage()
    {
        ViewModel = App.GetService<LoginViewModel>();
        InitializeComponent();
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        string username = UsernameTextBox.Text;
        string password = PasswordBox.Password;

        // Tutaj dodaj kod do weryfikacji danych logowania
        // Na przykład sprawdź dane w bazie danych

        if (username == "admin" && password == "admin") // Przykładowa weryfikacja (niebezpieczna)
        {
            // Logowanie udane, możesz przenieść użytkownika do kolejnej strony
            // Na przykład:
            // this.Frame.Navigate(typeof(NextPage));
            this.Frame.Navigate(typeof(GamesPage));
        }
        else
        {
            // Logowanie nieudane, wyświetl odpowiednie komunikaty
            // Na przykład:
            // MessageBox.Show("Błąd logowania. Spróbuj ponownie.");
        }
    }
}
