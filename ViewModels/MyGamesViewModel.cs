using System.Collections.ObjectModel;
using System.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using SGSClient.Core.Authorization;
using SGSClient.Core.Database;
using SGSClient.Core.Extensions;

namespace SGSClient.ViewModels;

public partial class MyGamesViewModel : ObservableRecipient
{
    private ObservableCollection<GamesViewModel> _gamesList;
    private readonly IAppUser _appUser;

    public ObservableCollection<GamesViewModel> GamesList
    {
        get => _gamesList;
        private set => SetProperty(ref _gamesList, value);
    }
    public MyGamesViewModel(IAppUser appUser)
    {
        GamesList = new ObservableCollection<GamesViewModel>();
        _appUser = appUser;
    }

    public async Task LoadMyGamesFromDatabaseAsync()
    {
        try
        {
            GamesList = new ObservableCollection<GamesViewModel>(await LoadMyGamesFromDBAsync());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while loading games: {ex.Message}");
        }
    }

    public async Task<List<GamesViewModel>> LoadMyGamesFromDBAsync()
    {
        List<GamesViewModel> gamesList = [];
        var dataSet = db.con.select(SqlQueries.gamesUserInfo, _appUser.UserId);
        if (dataSet.Tables[0].Rows.Count > 0)
        {
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                GamesViewModel game = new GamesViewModel(
                    gameId: row["GameId"].ToString(),
                    gameSymbol: row["GameSymbol"].ToString(),
                    gameTitle: row["Title"].ToString(),
                    gamePayloadName: row["PayloadName"].ToString(),
                    gameExeName: row["ExeName"].ToString(),
                    gameZipLink: row["ZipLink"].ToString(),
                    gameVersionLink: row["VersionLink"].ToString(),
                    gameDescription: row["Description"].ToString(),
                    hardwareRequirements: row["HardwareRequirements"].ToString(),
                    otherInformations: row["OtherInformation"].ToString(),
                    gameDeveloper: row["GameDeveloper"].ToString(),
                    logoPath: row["LogoPath"].ToString(),
                    gameType: row["GameType"].ToString(),
                    draftP: row["DraftP"].ToString()
                );

                gamesList.Add(game);
            }
            return gamesList;
        }
        else
            return gamesList;
    }

}
