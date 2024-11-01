using Microsoft.Data.SqlClient;
using CommunityToolkit.Mvvm.ComponentModel;
using SGSClient.Core.Database;
using SGSClient.Helpers;
using System.Data;
using SGSClient.Contracts.Services;

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
        private readonly INavigationService _navigationService;
        #endregion

        #region Constructor
        public MyAccountViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }
        #endregion

        #region Fields
        private string? _avatarUrl;
        private string? _welcomeText;
        #endregion

        #region Methods
        public async void LoadUserData(string userId)
        {
            string email = "test@test.com";
            string username;

            using SqlConnection con = db.Connect();
            try
            {
                Console.WriteLine($"Connection state before opening: {con.State}");

                if (con.State != ConnectionState.Open)
                {
                    await con.OpenAsync();
                    Console.WriteLine("Connection opened.");
                }

                string query = @"
    select
      r.Email,
      d.Name
    from Registration r
    inner join sgsDevelopers d on d.Id = r.DeveloperId
    where r.Id = @p0";

                object[] parameters = { userId };
                DataSet result;

                try
                {
                    result = await db.SelectSQLAsync(con, query, parameters);
                }
                catch (Exception sqlEx)
                {
                    Console.WriteLine($"Error executing SQL command: {sqlEx.Message}");
                    return;
                }

                if (result.Tables.Count > 0 && result.Tables[0].Rows.Count > 0)
                {
                    DataRow row = result.Tables[0].Rows[0];
                    email = row["Email"].ToString();
                    username = row["Name"].ToString();

                    AvatarUrl = GravatarHelper.GetAvatarUrl(email);
                    WelcomeText = "Witaj, " + username + "!";
                }
                else
                {
                    Console.WriteLine($"Nie znaleziono użytkownika o Id: {userId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Wystąpił błąd podczas ładowania danych użytkownika: {ex.Message}");
            }

            AvatarUrl = GravatarHelper.GetAvatarUrl(email);
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
