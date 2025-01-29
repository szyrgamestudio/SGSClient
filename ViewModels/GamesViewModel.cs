using System.Collections.ObjectModel;
using System.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using SGSClient.Controllers;
using SGSClient.Core.Database;

namespace SGSClient.ViewModels
{
    public class GamesViewModel : ObservableRecipient
    {

        private readonly ConfigurationManagerSQL _configManagerSQL;
        public string _gameId;
        private string _gameSymbol;
        private string _gameName;
        private string _gameDeveloper;
        private string _gameTitle;
        private Uri _imageSource;
        private string _gameVersion;
        private string _gamePayloadName;
        private string _gameExeName;
        private string _gameZipLink;
        private string _gameVersionLink;
        private string _gameDescription;
        private string _hardwareRequirements;
        private string _otherInformations;
        private string _logoPath;
        private string _gameType;
        private string _draftP;
        private ConfigurationManagerSQL configManagerSQL;

        public string GameId
        {
            get => _gameId;
            set => SetProperty(ref _gameId, value);
        }
        public string GameSymbol
        {
            get => _gameSymbol;
            set => SetProperty(ref _gameSymbol, value);
        }
        public string GameName
        {
            get => _gameName;
            set => SetProperty(ref _gameName, value);
        }

        public string GameDeveloper
        {
            get => _gameDeveloper;
            set => SetProperty(ref _gameDeveloper, value);
        }

        public string GameTitle
        {
            get => _gameTitle;
            set => SetProperty(ref _gameTitle, value);
        }

        public Uri ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }

        public string GameVersion
        {
            get => _gameVersion;
            set => SetProperty(ref _gameVersion, value);
        }

        public string GamePayloadName
        {
            get => _gamePayloadName;
            set => SetProperty(ref _gamePayloadName, value);
        }

        public string GameExeName
        {
            get => _gameExeName;
            set => SetProperty(ref _gameExeName, value);
        }

        public string GameZipLink
        {
            get => _gameZipLink;
            set => SetProperty(ref _gameZipLink, value);
        }

        public string GameVersionLink
        {
            get => _gameVersionLink;
            set => SetProperty(ref _gameVersionLink, value);
        }

        public string GameDescription
        {
            get => _gameDescription;
            set => SetProperty(ref _gameDescription, value);
        }

        public string HardwareRequirements
        {
            get => _hardwareRequirements;
            set => SetProperty(ref _hardwareRequirements, value);
        }

        public string OtherInformations
        {
            get => _otherInformations;
            set => SetProperty(ref _otherInformations, value);
        }

        public string LogoPath
        {
            get => _logoPath;
            set => SetProperty(ref _logoPath, value);
        }

        public string GameType
        {
            get => _gameType;
            set => SetProperty(ref _gameType, value);
        }
        public string DraftP
        {
            get => _draftP;
            set => SetProperty(ref _draftP, value);
        }
        public GamesViewModel(string gameId, string gameSymbol, string gameTitle, string gamePayloadName, string gameExeName,
            string gameZipLink, string gameVersionLink, string gameDescription, string hardwareRequirements, string otherInformations, string gameDeveloper, string logoPath, string gameType, string draftP)
        {
            GameId = gameId;
            GameSymbol = gameSymbol;
            GameTitle = gameTitle;
            GamePayloadName = gamePayloadName;
            GameExeName = gameExeName;
            GameZipLink = gameZipLink;
            GameVersionLink = gameVersionLink;
            GameDescription = gameDescription;
            HardwareRequirements = hardwareRequirements;
            OtherInformations = otherInformations;
            GameDeveloper = gameDeveloper;
            LogoPath = logoPath;
            GameType = gameType;
            DraftP = draftP;
        }

        private readonly DbContext _dbContext;

        private ObservableCollection<GamesViewModel> _gamesList;
        private ObservableCollection<GamesViewModel> _gamesFeaturedList;

        public ObservableCollection<GamesViewModel> GamesList
        {
            get => _gamesList;
            private set => SetProperty(ref _gamesList, value);
        }

        public ObservableCollection<GamesViewModel> GamesFeaturedList
        {
            get => _gamesFeaturedList;
            private set => SetProperty(ref _gamesFeaturedList, value);
        }

        public GamesViewModel(DbContext dbContext)
        {
            _dbContext = dbContext;
            GamesList = new ObservableCollection<GamesViewModel>();
            GamesFeaturedList = new ObservableCollection<GamesViewModel>();
        }

        public async Task LoadGamesFromDatabaseAsync()
        {
            try
            {
                GamesList = new ObservableCollection<GamesViewModel>(await LoadGamesFromDatabaseAsync(false));
                GamesFeaturedList = new ObservableCollection<GamesViewModel>(await LoadFeaturedGamesFromDatabaseAsync(false));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while loading games: {ex.Message}");
            }
        }

        public async Task<List<GamesViewModel>> LoadGamesFromDatabaseAsync(bool bypassDraftP)
        {
            List<GamesViewModel> gamesList = [];
            var dataSet = await _dbContext.ExecuteQueryAsync(SqlQueries.gamesInfo, bypassDraftP);
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
        public async Task<List<GamesViewModel>> LoadFeaturedGamesFromDatabaseAsync(bool bypassDraftP)
        {
            List<GamesViewModel> gamesList = [];
            var dataSet = await _dbContext.ExecuteQueryAsync(SqlQueries.gamesFeaturedInfo, bypassDraftP);
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
}
