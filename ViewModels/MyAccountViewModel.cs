using System.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using SGSClient.Contracts.Services;
using SGSClient.Core.Authorization;
using SGSClient.Core.Database;
namespace SGSClient.ViewModels
{
    public partial class MyAccountViewModel(INavigationService navigationService, DbContext dbContext, IAppUser appUser) : ObservableRecipient
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
        private readonly DbContext _dbContext = dbContext;
        private readonly INavigationService _navigationService = navigationService;
        private readonly IAppUser _appUser = appUser;

        #endregion

        #region Methods
        public async void LoadUserData()
        {
            string? email;
            string? username;

            var dataSet = await _dbContext.ExecuteQueryAsync(SqlQueries.userDetailsSql, _appUser.UserId);
            if (dataSet.Tables[0].Rows.Count == 0)
                return;

            DataRow row = dataSet.Tables[0].Rows[0];
            email = row["Email"] == DBNull.Value ? string.Empty : row["Email"].ToString();
            username = row["Name"] == DBNull.Value ? string.Empty : row["Name"].ToString();

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