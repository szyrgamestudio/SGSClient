using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Data.SqlClient;
using System.Security.Cryptography;
using SGSClient.Core.Database;
using SGSClient.ViewModels;

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
    /// Obsługa zdarzenia kliknięcia przycisku "Resetuj hasło".
    /// </summary>
    private void buttonResetPassword_Click(object sender, RoutedEventArgs e)
    {
        string email = textBoxEmail.Text;

        if (!string.IsNullOrWhiteSpace(email) && IsValidEmail(email))
        {
            // Wygeneruj nowe hasło
            string newPassword = GenerateNewPassword();
            string securePassword = HashPassword(newPassword);

            // Aktualizuj hasło w bazie danych
            UpdatePasswordInDatabase(email, securePassword);

            // Wysyłanie e-maila z nowym hasłem
            SendNewPasswordEmail(email, newPassword);
        }
        else
        {
            errorMessage.Text = "Wprowadź poprawny adres e-mail";
        }
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
    /// Szyfruje hasło za pomocą algorytmu SHA-256.
    /// </summary>
    private string HashPassword(string password)
    {
        // Generate a random salt
        byte[] salt;
        new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

        // Append the salt to the password
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] saltedPassword = new byte[passwordBytes.Length + salt.Length];
        Buffer.BlockCopy(passwordBytes, 0, saltedPassword, 0, passwordBytes.Length);
        Buffer.BlockCopy(salt, 0, saltedPassword, passwordBytes.Length, salt.Length);

        // Compute the hash using SHA-256 with salt
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] hashBytes = sha256Hash.ComputeHash(saltedPassword);

            // Convert the hash bytes to a hexadecimal string
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                builder.Append(hashBytes[i].ToString("x2"));
            }

            // Append the salt to the hashed password
            builder.Append(Convert.ToBase64String(salt));

            return builder.ToString();
        }
    }

    /// <summary>
    /// Aktualizuje hasło użytkownika w bazie danych.
    /// </summary>
    private void UpdatePasswordInDatabase(string email, string newPassword)
    {
        // Połączenie do bazy danych
        using (SqlConnection connection = new SqlConnection(db.con))
        {
            try
            {
                connection.Open();

                // Zapytanie SQL do aktualizacji hasła dla użytkownika o danym adresie e-mail
                string query = "update Registration set Password = @NewPassword WHERE Email = @Email";

                // Utwórz nowy obiekt SqlCommand z zapytaniem SQL i połączeniem
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Dodaj parametry do zapytania SQL
                    command.Parameters.AddWithValue("@NewPassword", newPassword);
                    command.Parameters.AddWithValue("@Email", email);

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
    /// <summary>
    /// Wysyła wiadomość e-mail z nowym hasłem.
    /// </summary>
    private void SendNewPasswordEmail(string email, string newPassword)
    {
        // Ustawienia serwera SMTP
        string smtpServer = "smtp-mail.outlook.com";
        int smtpPort = 587;
        string smtpUsername = "rbarczynski000@outlook.com";
        string smtpPassword = "m)WJ\"w)CVxiz/.2";

        // Adres e-mail nadawcy
        string fromEmail = "rbarczynski000@outlook.com";

        // Tworzenie klienta SMTP
        using (SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort))
        {
            smtpClient.EnableSsl = true; // Włącz SSL/TLS
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

            // Tworzenie wiadomości e-mail
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(fromEmail);
            mailMessage.To.Add(email);
            mailMessage.Subject = "Nowe hasło";

            // Treść e-maila
            StringBuilder body = new StringBuilder();
            body.AppendLine("Twoje nowe hasło zostało wygenerowane.");
            body.AppendLine($"Nowe hasło: {newPassword}");
            mailMessage.Body = body.ToString();

            // Wysłanie wiadomości e-mail
            smtpClient.Send(mailMessage);
        }
    }

    /// <summary>
    /// Sprawdza, czy podany adres e-mail jest poprawny.
    /// </summary>
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    #endregion
}
