using System.Net;
using System.Net.Mail;
using System.Text;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SGSClient.Core.Database;
using SGSClient.Core.Extensions;
using SGSClient.Core.Interface;

namespace SGSClient.ViewModels;

public partial class ForgotPasswordViewModel : ObservableRecipient
{
    private string _email;
    private string _newPassword;
    private string _repeatPassword;
    private string _errorMessage;
    private readonly IPasswordHasher _passwordHasher;
    private bool _isTokenValid;

    public string Email
    {
        get => _email;
        set { _email = value; OnPropertyChanged(); }
    }
    public string NewPassword
    {
        get => _newPassword;
        set { _newPassword = value; OnPropertyChanged(); }
    }
    public string RepeatPassword
    {
        get => _repeatPassword;
        set { _repeatPassword = value; OnPropertyChanged(); }
    }
    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); }
    }
    public bool IsTokenValid
    {
        get => _isTokenValid;
        set
        {
            _isTokenValid = value;
            OnPropertyChanged();
        }
    }

    public ICommand SendResetCodeCommand { get; }

    public ForgotPasswordViewModel(IPasswordHasher passwordHasher)
    {
        SendResetCodeCommand = new RelayCommand<string>(SendResetCode);
        _passwordHasher = passwordHasher;
    }

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
    public async Task TokenValidation(string email, string token)
    {
        var dataSet = db.con.select(SqlQueries.userCheckToken, email);
        if (dataSet.Tables[0].Rows.Count > 0)
        {
            string hashedPasswordWithSalt = dataSet.Tables[0].Rows[0]["AccessToken"].ToString();
            string storedHashedToken = hashedPasswordWithSalt[..64];
            string storedSalt = hashedPasswordWithSalt[64..];

            string enteredToken = _passwordHasher.HashPasswordWithSalt(token, Convert.FromBase64String(storedSalt));
            if (enteredToken == storedHashedToken)
            {
                IsTokenValid = true;
                return;
            }
        }
    }

    public async void SendResetCode(string email)
    {
        if (!IsValidEmail(email))
        {
            ErrorMessage = "Wprowadź poprawny adres e-mail";
            return;
        }

        string accessToken = Guid.NewGuid().ToString()[..8];
        string tempPassword = _passwordHasher.HashPassword(accessToken);

        await UpdateTokenInDatabaseAsync(email, tempPassword);
        await SendAccessTokenEmailAsync(email, accessToken);
    } //Button => "Wyślij kod dostępu"

    public async Task UpdateTokenInDatabaseAsync(string email, string accessToken)
    {
        var existingUserDataSet = db.con.select(SqlQueries.checkUserSql, email);
        if (existingUserDataSet.Tables[0].Rows.Count == 0)
            return;

        try
        {
            db.con.select(SqlQueries.userUpdateToken, accessToken, email);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while updating token: {ex.Message}");
            ErrorMessage = "An error occurred while updating token. Please try again.";
        }
    }
    public async Task UpdatePasswordInDatabase(string email, string newPassword)
    {
        var existingUserDataSet = db.con.select(SqlQueries.checkUserSql, email);
        if (existingUserDataSet.Tables[0].Rows.Count == 0) return;

        try
        {
            db.con.select(SqlQueries.userUpdatePass, newPassword, email);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Wystąpił błąd podczas aktualizacji hasła w bazie danych: {ex.Message}");
            ErrorMessage = "Nie udało się zaktualizować hasła dla użytkownika {email}.";
        }
    }

    #region Mailing
    private async Task SendAccessTokenEmailAsync(string email, string accessToken)
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
                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, "SGSClient"),
                    Subject = "Twój nowy token resetu hasła",
                    Body = CreateEmailBody(accessToken)
                };
                mailMessage.To.Add(email);

                // Wysłanie wiadomości e-mail asynchronicznie
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Wystąpił błąd podczas wysyłania e-maila: {ex.Message}");
            }
        }
    }
    private static string CreateEmailBody(string accessToken)
    {
        // Treść e-maila
        StringBuilder body = new StringBuilder();
        body.AppendLine("Cześć,\n");
        body.AppendLine("Informujemy, że na Twoje żądanie został wygenerowany nowy token resetu hasła.\n");
        body.AppendLine($"Twój token resetu hasła: {accessToken}\n");
        body.AppendLine("Jeśli to nie Ty prosiłeś/aś o reset hasła, zignoruj tę wiadomość i daj nam znać, abyśmy mogli zabezpieczyć Twoje konto.\n\n");
        body.AppendLine("Z poważaniem,");
        body.AppendLine("SGS");
        return body.ToString();
    }
    #endregion
}
