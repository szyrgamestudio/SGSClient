using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Data.SqlClient;
using SGSClient.Contracts.Services;
using SGSClient.Core.Authorization;
using SGSClient.Core.Database;
using System.Data;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Windows.System;

namespace SGSClient.ViewModels
{
    public partial class LoginViewModel : ObservableRecipient
    {
        #region Properties
        public string Email { get; set; }
        public string Password { get; set; }
        public string ErrorMessage { get; set; }
        private readonly INavigationService _navigationService;

        #endregion

        #region Constructor
        public LoginViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }
        #endregion

        #region Public Methods
        public async Task CheckUserSessionAsync()
        {
            await Task.Delay(100);

            if (AppSession.CurrentUserSession.IsLoggedIn)
            {
                _navigationService.NavigateTo(typeof(MyAccountViewModel).FullName!);
            }
        }
        public async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Adres e-mail jest wymagany.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Hasło jest wymagane.";
                return;
            }

            if (!IsValidEmail(Email))
            {
                ErrorMessage = "Wprowadź prawidłowy adres e-mail.";
                return;
            }

            await AttemptLoginAsync();
        }
        private static bool IsValidEmail(string email)
        {
            try
            {
                MailAddress mailAddress = new MailAddress(email);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
        public void NavigateToRegister()
        {
            _navigationService.NavigateTo(typeof(RegisterViewModel).FullName!);
        }
        public void NavigateToForgotPassword()
        {
            _navigationService.NavigateTo(typeof(ForgotPasswordViewModel).FullName!);
        }

        #endregion

        #region Private Methods
        private async Task AttemptLoginAsync()
        {
            using SqlConnection con = db.Connect();
            try
            {
                Console.WriteLine($"Connection state before opening: {con.State}");

                if (con.State != ConnectionState.Open)
                {
                    await con.OpenAsync();
                    Console.WriteLine("Connection opened.");
                }

                string sqlQuery = @"
            select
              r.Id,
              r.Password
            from Registration r
            where r.Email = @p0";

                object[] parameters = { Email };

                DataSet result;

                try
                {
                    result = await db.SelectSQLAsync(con, sqlQuery, parameters);
                }
                catch (Exception sqlEx)
                {
                    ErrorMessage = $"Error executing SQL command: {sqlEx.Message}";
                    return;
                }

                if (result.Tables.Count > 0 && result.Tables[0].Rows.Count > 0)
                {
                    DataRow row = result.Tables[0].Rows[0];
                    string userId = row["Id"].ToString();
                    var storedPasswordHash = row["Password"].ToString();
                    var storedSalt = storedPasswordHash[64..];

                    if (VerifyPassword(Password, storedPasswordHash.Substring(0, 64), storedSalt))
                    {
                        AppSession.CurrentUserSession.IsLoggedIn = true;
                        AppSession.CurrentUserSession.UserId = userId;
                        SessionManager.SaveSession(AppSession.CurrentUserSession);
                        _navigationService.NavigateTo(typeof(MyAccountViewModel).FullName!);
                    }
                    else
                    {
                        ErrorMessage = "Nieprawidłowe hasło.";
                    }
                }
                else
                {
                    ErrorMessage = "Nieprawidłowe dane logowania.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to connect to the database: {ex.Message} \n {ex.InnerException?.Message}";
            }
        }
        private static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            string hashedPassword = HashPasswordWithSalt(password, Convert.FromBase64String(storedSalt));
            return hashedPassword == storedHash;
        }
        private static string HashPasswordWithSalt(string password, byte[] salt)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] saltedPassword = new byte[passwordBytes.Length + salt.Length];
            Buffer.BlockCopy(passwordBytes, 0, saltedPassword, 0, passwordBytes.Length);
            Buffer.BlockCopy(salt, 0, saltedPassword, passwordBytes.Length, salt.Length);

            using SHA256 sha256Hash = SHA256.Create();
            byte[] hashBytes = sha256Hash.ComputeHash(saltedPassword);

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                builder.Append(hashBytes[i].ToString("x2"));
            }

            return builder.ToString();
        }

        #endregion
    }
}
