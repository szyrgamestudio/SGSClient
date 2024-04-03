using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using SGSClient.ViewModels;
using System.Data.SqlClient;

namespace SGSClient.Views;

public sealed partial class RegisterPage : Page
{
    public RegisterViewModel ViewModel
    {
        get;
    }

    public RegisterPage()
    {
        ViewModel = App.GetService<RegisterViewModel>();
        InitializeComponent();
    }

    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        string username = UsernameTextBox.Text;
        string password = PasswordBox.Password;
        string email = EmailTextBox.Text;
        string firstName = FirstNameTextBox.Text;
        string lastName = LastNameTextBox.Text;

        try
        {
            // Utwórz połączenie z bazą danych
            using (SqlConnection connection = new SqlConnection("connection_string"))
            {
                // Otwórz połączenie
                await connection.OpenAsync();

                // Utwórz komendę SQL dla procedury rejestracji
                using (SqlCommand command = new SqlCommand("RegisterUser", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    // Dodaj parametry do komendy
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@FirstName", firstName);
                    command.Parameters.AddWithValue("@LastName", lastName);

                    // Wykonaj procedurę rejestracji
                    await command.ExecuteNonQueryAsync();

                    // Wyświetl komunikat o pomyślnej rejestracji
                    // Możesz użyć MessageBox.Show() lub innej metody do wyświetlania komunikatów
                    // MessageBox.Show("Rejestracja zakończona pomyślnie.");
                }
            }
        }
        catch (Exception ex)
        {
            // Obsłuż błąd rejestracji, np. wyświetlając komunikat o błędzie
            // MessageBox.Show("Błąd rejestracji: " + ex.Message);
        }
    }
}
