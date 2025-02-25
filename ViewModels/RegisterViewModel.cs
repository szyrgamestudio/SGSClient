using System.Data;
using System.Net.Mail;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Windows.ApplicationModel.Resources;
using SGSClient.Contracts.Services;
using SGSClient.Core.Database;
using SGSClient.Core.Extensions;
using SGSClient.Core.Interface;

namespace SGSClient.ViewModels
{
    public partial class RegisterViewModel : ObservableRecipient
    {
        #region Properties
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string PasswordConfirmed { get; set; }
        public string ErrorMessage { get; set; }
        private readonly INavigationService _navigationService;
        private readonly IPasswordHasher _passwdHasher;
        #endregion

        #region Constructor
        public RegisterViewModel(INavigationService navigationService, IPasswordHasher passwordHasher)
        {
            _navigationService = navigationService;
            _passwdHasher = passwordHasher;
        }
        #endregion

        #region Public Methods
        public async Task RegisterAsync()
        {
            var resourceLoader = new ResourceLoader();

            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = resourceLoader.GetString("LoginPage_Error_EmailReq");
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = resourceLoader.GetString("LoginPage_Error_PasswdReq");
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordConfirmed))
            {
                ErrorMessage = resourceLoader.GetString("RegisterPage_Error_PasswdConfirmReq");
                return;
            }

            if (!IsValidEmail(Email))
            {
                ErrorMessage = resourceLoader.GetString("LoginPage_Error_EmailInvalid");
                return;
            }

            if (PasswordConfirmed != Password)
            {
                ErrorMessage = resourceLoader.GetString("RegisterPage_Error_PasswdDiff");
                return;
            }

            await AttemptRegisterAsync();

        }
        #endregion

        #region Private Methods
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
        private Task AttemptRegisterAsync()
        {
            DataSet ds = db.con.select(@"
select
  r.Id
from Registration r
where r.Email = @p0
", Email);

            if (ds.Tables[0].Rows.Count != 0)
                return Task.CompletedTask;

            db.con.exec(@"
insert sgsDevelopers (Name)
select
  @p0

declare @devId int = SCOPE_IDENTITY()

insert Registration (Email, Password, RegistrationOnTime, DeveloperId)
select
  @p1
, @p2
, @p3
, @devId
", Username, Email, _passwdHasher.HashPassword(Password), DateTime.Now.ToSqlParameter());

            _navigationService.NavigateTo(typeof(LoginViewModel).FullName!);


            return Task.CompletedTask;
        }
        #endregion
    }
}