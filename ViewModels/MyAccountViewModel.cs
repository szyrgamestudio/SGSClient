using Microsoft.Data.SqlClient;
using CommunityToolkit.Mvvm.ComponentModel;
using SGSClient.Core.Database;
using SGSClient.Helpers;
using System.Data;
using SGSClient.Contracts.Services;
using Windows.UI;

namespace SGSClient.ViewModels
{
    public partial class MyAccountViewModel : ObservableRecipient
    {
        #region Properties
        public string AvatarUrl
        {
            get => _avatarUrl;
            set => SetProperty(ref _avatarUrl, value);
        }
        public string WelcomeText
        {
            get => _welcomeText;
            set => SetProperty(ref _welcomeText, value);
        }
        private readonly DbContext _dbContext;
        private readonly INavigationService _navigationService;
        #endregion

        #region Constructor
        public MyAccountViewModel(INavigationService navigationService, DbContext dbContext)
        {
            _navigationService = navigationService;
            _dbContext = dbContext;
        }
        #endregion

        #region Fields
        private string? _avatarUrl;
        private string? _welcomeText;
        #endregion

        #region Methods
        public async void LoadUserData(string userId)
        {
            string email;
            string username;

            var dataSet = await _dbContext.ExecuteQueryAsync(SqlQueries.userDetailsSql, userId);
            if (dataSet.Tables[0].Rows.Count == 0)
                return;

            DataRow row = dataSet.Tables[0].Rows[0];
            email = row["Email"].ToString();
            username = row["Name"].ToString();

            AvatarUrl = GravatarHelper.GetAvatarUrl(email);
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
