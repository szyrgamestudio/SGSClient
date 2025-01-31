using System.Data;
using System.Net.Mail;
using CommunityToolkit.Mvvm.ComponentModel;
using SGSClient.Contracts.Services;
using SGSClient.Core.Authorization;
using SGSClient.Core.Database;
using SGSClient.Core.Extensions;
using SGSClient.Services;

namespace SGSClient.ViewModels
{
    public partial class LoginViewModel : ObservableRecipient
    {
        #region Properties
        public string Email { get; set; }
        public string Password { get; set; }
        public string ErrorMessage { get; set; }
        private readonly INavigationService _navigationService;
        private readonly PasswordHasher _passwdHasher;
        private readonly IAppUser _appUser;

        #endregion

        #region Constructor
        public LoginViewModel(INavigationService navigationService, PasswordHasher passwordHasher, IAppUser appUser)
        {
            _navigationService = navigationService;
            _passwdHasher = passwordHasher;
            _appUser = appUser;
        }

        #endregion

        #region Public Methods
        public async Task CheckUserSessionAsync()
        {
            await Task.Delay(100);

            if (_appUser.IsLoggedIn)
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
        private Task AttemptLoginAsync()
        {
            DataSet ds = db.con.select(@"
select
  r.Id
, r.Password
from Registration r
where r.Email = @p0
", Email);
            if (ds.Tables[0].Rows.Count == 0)
                return Task.CompletedTask;
            DataRow dr = ds.Tables[0].Rows[0];
            int userId = dr.TryGetValue("Id");
            string storedPasswordHash = dr.TryGetValue("Password:");
            var storedSalt = storedPasswordHash[64..];

            if (VerifyPassword(Password, storedPasswordHash.Substring(0, 64), storedSalt))
            {
                _appUser.IsLoggedIn = true;
                _appUser.UserId = userId;
                _appUser.SaveSession();
                _navigationService.NavigateTo(typeof(MyAccountViewModel).FullName!);
            }
            else
            {
                ErrorMessage = "Nieprawidłowe hasło.";
            }

            return Task.CompletedTask;
        }
        private bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            string hashedPassword = _passwdHasher.HashPasswordWithSalt(password, Convert.FromBase64String(storedSalt));
            return hashedPassword == storedHash;
        }
        #endregion
    }
}