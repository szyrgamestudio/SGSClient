using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Data.SqlClient;
using SGSClient.Contracts.Services;
using SGSClient.Core.Authorization;
using SGSClient.Core.Database;
using SGSClient.Services;
using System.Data;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace SGSClient.ViewModels
{
    public partial class LoginViewModel : ObservableRecipient
    {
        #region Properties
        public string Email { get; set; }
        public string Password { get; set; }
        public string ErrorMessage { get; set; }
        private readonly INavigationService _navigationService;
        private readonly DbContext _dbContext;
        private readonly PasswordHasher _passwdHasher;
        #endregion

        #region Constructor
        public LoginViewModel(INavigationService navigationService, DbContext dbContext, PasswordHasher passwordHasher)
        {
            _navigationService = navigationService;
            _dbContext = dbContext;
            _passwdHasher = passwordHasher;
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
            var dataSet = await _dbContext.ExecuteQueryAsync(SqlQueries.checkUserSql, Email);
            if (dataSet.Tables[0].Rows.Count == 0)
                return;

            DataRow row = dataSet.Tables[0].Rows[0];
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
        private bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            string hashedPassword = _passwdHasher.HashPasswordWithSalt(password, Convert.FromBase64String(storedSalt));
            return hashedPassword == storedHash;
        }

        #endregion
    }
}