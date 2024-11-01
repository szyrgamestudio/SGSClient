using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Net.Mail;
using System.Net;
using System.Text;
using Microsoft.Data.SqlClient;
using SGSClient.Core.Database;
using SGSClient.ViewModels;
using SGSClient.Services;
using System.Data;
using Microsoft.UI.Xaml.Media.Animation;
using SGSClient.Core;

namespace SGSClient.Views;
public sealed partial class ForgotPasswordPage : Page
{
    #region Properties

    /// <summary>
    /// Właściwość ViewModel dla strony resetowania hasła.
    /// </summary>
    public ForgotPasswordViewModel ViewModel
    {
        get;
    }
    #endregion
    #region Constructors

    /// <summary>
    /// Konstruktor klasy ForgotPasswordPage.
    /// </summary>
    public ForgotPasswordPage()
    {
        ViewModel = App.GetService<ForgotPasswordViewModel>();
        InitializeComponent();
    }
    #endregion
    #region Event Handlers

    /// <summary>
    /// Obsługa zdarzenia kliknięcia przycisku "Wyślij kod resetu".
    /// </summary>
    private void buttonSendResetCodePassword_Click(object sender, RoutedEventArgs e)
    {
        IPasswordHasher passwordHasher = new PasswordHasher();

        string email = textBoxEmail.Text;

        if (!string.IsNullOrWhiteSpace(email) && IsValidEmail(email))
        {
            // Wygeneruj nowe hasło
            string newPassword = GenerateNewPassword();
            string securePassword = passwordHasher.HashPassword(newPassword);

            // Aktualizuj hasło w bazie danych
            UpdateTokenInDatabase(email, securePassword);

            // Wysyłanie e-maila z nowym hasłem
            SendAccessTokenEmail(email, newPassword);
        }
        else
        {
            errorMessage.Message = "Wprowadź poprawny adres e-mail";
            errorMessage.Visibility = Visibility.Visible;
            errorMessage.IsOpen = true;
        }
    }

    /// <summary>
    /// Obsługa zdarzenia kliknięcia przycisku "Resetuj hasło".
    /// </summary>
    private void buttonResetPassword_Click(object sender, RoutedEventArgs e)
    {
        string email = textBoxEmail.Text;
        string token = textBoxAccessKey.Text;

        if (!string.IsNullOrWhiteSpace(email) && IsValidEmail(email) && !string.IsNullOrWhiteSpace(token))
        {
            // Sprawdź poprawność tokenu (może być za pomocą bazy danych lub innej metody weryfikacji)
            if (IsTokenValid(email, token))
            {
                // Pokaż pola do wprowadzenia nowego hasła
                labelNewPassword.Visibility = Visibility.Visible;
                textBoxNewPassword.Visibility = Visibility.Visible;
                labelRepeatPassword.Visibility = Visibility.Visible;
                textBoxRepeatPassword.Visibility = Visibility.Visible;
                buttonSaveNewPassword.Visibility = Visibility.Visible;
            }
            else
            {
                errorMessage.Message = "Niepoprawny kod resetu.";
                errorMessage.Visibility = Visibility.Visible;
                errorMessage.IsOpen = true;
            }
        }
        else
        {
            errorMessage.Message = "Wprowadź poprawny adres e-mail / kod resetu.";
            errorMessage.Visibility = Visibility.Visible;
            errorMessage.IsOpen = true;
        }
    }

    // Sprawdza, czy podany token jest poprawny dla danego e-maila
    private bool IsTokenValid(string email, string token)
    {
        using (SqlConnection con = db.Connect())
        {
            string query = "SELECT Id, AccessToken FROM [dbo].[Registration] WHERE Email = @Email";
            SqlCommand cmd = db.CommandSQL(con, query, email);
            IPasswordHasher passwordHasher = new PasswordHasher();

            // Wykonanie zapytania i pobranie danych za pomocą Db.SelectSQL
            DataSet dataSet = db.SelectSQL(cmd);

            // Sprawdzenie czy użytkownik istnieje w bazie
            if (dataSet.Tables[0].Rows.Count > 0)
            {
                // Pobranie hasła zaszyfrowanego oraz soli
                string hashedPasswordWithSalt = dataSet.Tables[0].Rows[0]["AccessToken"].ToString();
                string storedHashedToken = hashedPasswordWithSalt.Substring(0, 64); // Skrót hasła
                string storedSalt = hashedPasswordWithSalt.Substring(64); // Sól

                // Wygenerowanie skrótu na podstawie podanego hasła oraz soli z bazy
                string enteredToken = passwordHasher.HashPasswordWithSalt(token, Convert.FromBase64String(storedSalt));

                // Porównanie obliczonego skrótu hasła z przechowywanym skrótem
                if (enteredToken == storedHashedToken)
                {
                    buttonSendResetCodePassword.IsEnabled = false;
                    buttonResetPassword.IsEnabled = false;
                    return true;
                }
            }
        }
        return false;
    }

    #endregion
    #region Private Methods

    /// <summary>
    /// Generuje nowe hasło.
    /// </summary>
    private string GenerateNewPassword()
    {
        return Guid.NewGuid().ToString().Substring(0, 8);
    }

    /// <summary>
    /// Aktualizuje hasło użytkownika w bazie danych.
    /// </summary>
    private void UpdatePasswordInDatabase(string email, string newPassword)
    {
        // Połączenie do bazy danych
        using (SqlConnection connection = db.Connect())
        {
            try
            {
                // Zapytanie SQL do aktualizacji hasła dla użytkownika o danym adresie e-mail
                string query = "UPDATE Registration SET Password = @NewPassword WHERE Email = @Email";

                // Utwórz nowy obiekt SqlCommand z zapytaniem SQL i połączeniem
                using (SqlCommand command = db.CommandSQL(connection, query, newPassword, email))
                {
                    // Wykonaj zapytanie SQL
                    int rowsAffected = command.ExecuteNonQuery();

                    // Sprawdź, czy hasło zostało zaktualizowane poprawnie
                    if (rowsAffected > 0)
                    {
                        Console.WriteLine($"Hasło dla użytkownika {email} zostało zaktualizowane.");
                    }
                    else
                    {
                        Console.WriteLine($"Nie udało się zaktualizować hasła dla użytkownika {email}.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Wystąpił błąd podczas aktualizacji hasła w bazie danych: {ex.Message}");
            }
        }
    }
    private void UpdateTokenInDatabase(string email, string accessToken)
    {
        // Połączenie do bazy danych
        using (SqlConnection connection = db.Connect())
        {
            try
            {
                // Zapytanie SQL do aktualizacji tokenu dla użytkownika o danym adresie e-mail
                string query = "UPDATE Registration SET AccessToken = @AccessToken WHERE Email = @Email";

                // Utwórz nowy obiekt SqlCommand z zapytaniem SQL i połączeniem
                using (SqlCommand command = db.CommandSQL(connection, query, accessToken, email))
                {
                    // Wykonaj zapytanie SQL
                    int rowsAffected = command.ExecuteNonQuery();

                    // Sprawdź, czy token został zaktualizowany poprawnie
                    if (rowsAffected > 0)
                    {
                        Console.WriteLine($"Token użytkownika {email} został zaktualizowany.");
                    }
                    else
                    {
                        Console.WriteLine($"Nie udało się zaktualizować tokenu dla użytkownika {email}.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Wystąpił błąd podczas aktualizacji tokenu w bazie danych: {ex.Message}");
            }
        }
    }
    private void buttonSaveNewPassword_Click(object sender, RoutedEventArgs e)
    {
        string email = textBoxEmail.Text;
        string newPassword = textBoxNewPassword.Password;
        string repeatPassword = textBoxRepeatPassword.Password;

        if (newPassword == repeatPassword)
        {
            IPasswordHasher passwordHasher = new PasswordHasher();
            string securePassword = passwordHasher.HashPassword(newPassword);

            // Aktualizuj hasło w bazie danych
            UpdatePasswordInDatabase(email, securePassword);
            Frame.Navigate(typeof(LoginPage), new DrillInNavigationTransitionInfo());
        }
        else
        {
            errorMessage.Message = "Hasła się nie zgadzają.";
            errorMessage.Visibility = Visibility.Visible;
            errorMessage.IsOpen = true;
        }
    }

    /// <summary>
    /// Wysyła wiadomość e-mail z nowym hasłem.
    /// </summary>
    private void SendAccessTokenEmail(string email, string accessToken)
    {
        // Ustawienia serwera SMTP
        string smtpServer = "smtppro.zoho.eu";
        int smtpPort = 587;
        string smtpUsername = "sgsclient@massyn.dev";
        string smtpPassword = "EcM74if.!864Ps!";

        // Adres e-mail nadawcy
        string fromEmail = "sgsclient@massyn.dev";

        // Tworzenie klienta SMTP
        using (SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort))
        {
            try
            {
                smtpClient.EnableSsl = true; // Włącz SSL/TLS
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                // Tworzenie wiadomości e-mail
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(fromEmail, "SGSClient");
                mailMessage.To.Add(email);
                mailMessage.Subject = "Twój nowy token resetu hasła";

                // Treść e-maila
                StringBuilder body = new StringBuilder();
                body.AppendLine("Cześć,\n");
                body.AppendLine("Informujemy, że na Twoje żądanie został wygenerowany nowy token resetu hasła.\n");
                body.AppendLine($"Twój token resetu hasła: {accessToken}\n");
                body.AppendLine("Jeśli to nie Ty prosiłeś/aś o reset hasła, zignoruj tę wiadomość i daj nam znać, abyśmy mogli zabezpieczyć Twoje konto.\n\n");
                body.AppendLine("Z poważaniem,");
                body.AppendLine("SGS");
                mailMessage.Body = body.ToString();

                // Wysłanie wiadomości e-mail
                smtpClient.Send(mailMessage);
                //Frame.Navigate(typeof(LoginPage), new DrillInNavigationTransitionInfo());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
    }

    /// <summary>
    /// Sprawdza, czy podany adres e-mail jest poprawny.
    /// </summary>
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
