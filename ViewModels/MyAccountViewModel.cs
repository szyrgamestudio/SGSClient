using System.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using SGSClient.Contracts.Services;
using SGSClient.Core.Authorization;
using SGSClient.Core.Database;
using SGSClient.Core.Extensions;
namespace SGSClient.ViewModels
{
    public partial class MyAccountViewModel(INavigationService navigationService, IAppUser appUser) : ObservableRecipient
    {
        #region Fields
        private string? _avatarUrl;
        private string? _welcomeText;
        #endregion

        #region Properties
        public string? AvatarUrl
        {
            get => _avatarUrl;
            set => SetProperty(ref _avatarUrl, value);
        }
        public string? WelcomeText
        {
            get => _welcomeText;
            set => SetProperty(ref _welcomeText, value);
        }
        private readonly INavigationService _navigationService = navigationService;
        private readonly IAppUser _appUser = appUser;

        #endregion

        #region Methods
        public void LoadUserData()
        {
            string? email;
            string? username;

            DataSet ds = db.con.select(@"
select
  r.Email
, d.Name
from Registration r
inner join sgsDevelopers d on d.Id = r.DeveloperId
where r.Id = @p0
", _appUser.UserId);
            if (ds.Tables[0].Rows.Count == 0)
                return;

            DataRow dr = ds.Tables[0].Rows[0];
            email = dr.TryGetValue("Email") == DBNull.Value ? string.Empty : dr.TryGetValue("Email");
            username = dr.TryGetValue("Name") == DBNull.Value ? string.Empty : dr.TryGetValue("Name");

            AvatarUrl = _appUser.GetGravatar(email);
            WelcomeText = "Witaj, " + username + "!";
        }
        public void NavigateToUpload()
        {
            _navigationService.NavigateTo(typeof(UploadGameViewModel).FullName!);
        }
        public void NavigateToMyGames()
        {
            _navigationService.NavigateTo(typeof(MyGamesViewModel).FullName!);
        }
        #endregion
    }
}