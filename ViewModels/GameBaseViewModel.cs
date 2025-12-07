using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Kiota.Abstractions;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using SGSClient.Core.Authorization;
using SGSClient.Core.Database;
using SGSClient.Core.Extensions;
using SGSClient.Core.Utilities.AppInfoUtility.Interfaces;
using SGSClient.Core.Utilities.LogUtility;
using SGSClient.Models;
using SGSClient.Views;
using SQLite;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace SGSClient.ViewModels
{
    public partial class GameBaseViewModel : ObservableRecipient
    {
        #region Variables
        private readonly IAppUser _appUser;
        private readonly IAppInfo _appInfo;
        private readonly HttpClient httpClient = new();
        private static readonly string DatabasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "local_game_data.db");

        private int _ratingCount;
        private ObservableCollection<GameRating> _ratings = new();
        private double _avgRating;

        private string? gamepath;
        private int? gameId;
        private string? gameZip;
        private string? gameExe;
        private string? gameIdentifier;
        private string? gameName;
        private string? gameZipLink;
        private string gameVersion = "0.0.0";

        private GameImage _gameLogo;
        private ObservableCollection<GameImage> _gameImages = new();
        public ImageSource? BackgroundImage => GameImages.FirstOrDefault()?.ImageSource;


        #endregion

        #region Properties
        public int RatingCount
        {
            get => _ratingCount;
            set => SetProperty(ref _ratingCount, value);
        }

        public ObservableCollection<GameRating> Ratings
        {
            get => _ratings;
            set
            {
                if (SetProperty(ref _ratings, value))
                {
                    OnPropertyChanged(nameof(RatingsMinimal));
                }
            }
        }

        public double AvgRating
        {
            get => _avgRating;
            set { _avgRating = value; OnPropertyChanged(); }
        }
        private ObservableCollection<GameRating> _ratingsMinimal = new();
        public ObservableCollection<GameRating> RatingsMinimal
        {
            get => _ratingsMinimal;
            private set => SetProperty(ref _ratingsMinimal, value);
        }

        public int? GameId { get; private set; }
        public string? GameName { get; private set; }
        public string? GameIdentifier { get; private set; }
        public string? GameDeveloper { get; private set; }
        public string? GameDescription { get; private set; }
        public string? HardwareRequirements { get; private set; }
        public string? OtherInformations { get; private set; }
        public bool IsOtherInformationsVisible { get; private set; }
        public bool IsHardwareRequirementsVisible { get; private set; }
        public bool IsDLCVisible { get; private set; }
        public bool IsRatingVisible { get; private set; } = true;
        public bool IsAddRatingVisible { get; private set; }
        public string GameTime { get; private set; } = "0 min";

        public GameImage GameLogo
        {
            get => _gameLogo;
            set => SetProperty(ref _gameLogo, value);
        }

        public ObservableCollection<GameImage> GameLogos { get; set; } = new();
        public ObservableCollection<GameImage> GameImages
        {
            get => _gameImages;
            set => SetProperty(ref _gameImages, value);
        }

        public ObservableCollection<string> GameImagePaths { get; set; } = new();
        #endregion

        public GameBaseViewModel(IAppUser appUser, IAppInfo appInfo)
        {
            _appUser = appUser;
            _appInfo = appInfo;
        }



        #region Methods
        public async Task LoadGameData(string gameSymbol)
        {
            string nextcloudLogin = _appInfo.GetAppSetting("NextcloudLogin").Value;
            string nextcloudPassword = _appInfo.GetAppSetting("NextcloudPassword").Value;

            int userId = _appUser.GetCurrentUser().Id;

            DataSet ds = db.con.select(@"
/*0*/
select
  g.Id           [GameId]
, g.Title        [Title]
, g.Symbol       [GameSymbol]
, u.DisplayName  [GameDeveloper]
, gi.Url         [LogoPath]
, null	         [GameType]
, g.ExeName
, g.ZipLink
, g.CurrentVersion
, g.Description
, g.HardwareRequirements
, g.OtherInformation
, g.DraftP
from Games g
inner join Users u on u.Id = g.UserId
inner join GameImages gi on gi.GameId = g.Id and gi.LogoP = 1
where g.Symbol = @p0
order by g.Title

/*1*/
select
  gi.Url
from GameImages gi
inner join Games g on g.Id = gi.GameId
where g.Symbol = @p0 and gi.LogoP = 1

/*2*/
select
  gi.Url
from GameImages gi
inner join Games g on g.Id = gi.GameId
where g.Symbol = @p0 and gi.LogoP = 0
", gameSymbol);

            if (ds.Tables[0].Rows.Count > 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];
                gameId = dr.TryGetValue("GameId");
                gameZip = dr.TryGetValue("ZipLink");
                gameExe = dr.TryGetValue("ExeName");
                gameName = dr.TryGetValue("Title");
                gameIdentifier = dr.TryGetValue("GameSymbol");
                gameZipLink = dr.TryGetValue("ZipLink");
                gameVersion = dr.TryGetValue("CurrentVersion") ?? "0.0.0";

                LoadRatings(gameSymbol ?? "");
                LoadGameRatingsStats(gameSymbol ?? "");

                GameId = gameId;
                GameName = gameName;
                GameDeveloper = dr.TryGetValue("GameDeveloper") ?? "Brak dostępnych informacji.";
                GameDescription = dr.TryGetValue("Description") ?? "Brak dostępnych informacji.";
                GameIdentifier = dr.TryGetValue("GameSymbol") ?? null;
                HardwareRequirements = dr.TryGetValue("HardwareRequirements") ?? "Brak dostępnych informacji.";
                OtherInformations = dr.TryGetValue("OtherInformation");
                IsOtherInformationsVisible = !String.IsNullOrEmpty(dr.TryGetValue("OtherInformation"));
                IsHardwareRequirementsVisible = !String.IsNullOrEmpty(dr.TryGetValue("HardwareRequirements"));
                IsDLCVisible = false; //TODO
                IsAddRatingVisible = _appUser.GetCurrentUser().Id != default;
                DataSet dss = db.con.select(@"
/*0*/
select
  i.PlayTimeSec GameTime
from (select 1 x) x
left join UserGameInfo i on i.GameId = @p0 and i.UserId = @p1
", GameId.ToInt32(), userId);
                dr = dss.Tables[0].Rows[0];

                GameTime = FormatPlayTime(dr.TryGetValue("GameTime") ?? 0);

                OnPropertyChanged(nameof(GameTime));

                OnPropertyChanged(nameof(GameId));
                OnPropertyChanged(nameof(GameName));
                OnPropertyChanged(nameof(GameIdentifier));
                OnPropertyChanged(nameof(GameDeveloper));
                OnPropertyChanged(nameof(GameDescription));
                OnPropertyChanged(nameof(HardwareRequirements));
                OnPropertyChanged(nameof(OtherInformations));
                OnPropertyChanged(nameof(IsOtherInformationsVisible));
                OnPropertyChanged(nameof(IsHardwareRequirementsVisible));
                OnPropertyChanged(nameof(IsDLCVisible));
                OnPropertyChanged(nameof(IsAddRatingVisible));
                OnPropertyChanged(nameof(Ratings));

                GameLogos.Clear();
                GameImages.Clear();

                foreach (DataRow dr0 in ds.Tables[1].Rows)
                {
                    await LoadLogoFromNextcloud(dr0, nextcloudLogin, nextcloudPassword);
                }

                GameImages.Clear();
                foreach (DataRow dr1 in ds.Tables[2].Rows)
                {
                    await LoadImageFromNextcloud(dr1, nextcloudLogin, nextcloudPassword);
                }

                OnPropertyChanged(nameof(BackgroundImage));
            }
        }
        public (bool IsInstalled, bool IsUpdateAvailable) CheckForUpdate(string gameIdentifier)
        {
            try
            {
                string localVersion = GetLocalVersion(gameIdentifier ?? string.Empty);

                if (string.IsNullOrWhiteSpace(localVersion) || localVersion == "0.0.0")
                {
                    Debug.WriteLine("Brak zainstalowanej wersji gry.");
                    return (false, false);
                }

                string gameVersion = db.con.scalar(@"
select
  g.CurrentVersion
from Games g
where g.Symbol = @p0
", gameIdentifier);

                bool isUpdateAvailable = !string.Equals(localVersion, gameVersion, StringComparison.Ordinal);
                return (true, isUpdateAvailable);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd podczas sprawdzania aktualizacji: {ex.Message}");
                return (false, false);
            }
        }
        public async Task DownloadGameAsync(ShellPage shellPage)
        {
            if (string.IsNullOrWhiteSpace(gameName) ||
                string.IsNullOrWhiteSpace(gameZipLink) ||
                string.IsNullOrWhiteSpace(GameLogo?.Url) ||
                string.IsNullOrWhiteSpace(gameExe))
            {
                Debug.WriteLine("Brakuje wymaganych danych gry.");
                return;
            }

            StorageFolder? installFolder = null;

            if (StorageApplicationPermissions.FutureAccessList.ContainsItem($"GameInstallFolder_{gameIdentifier}"))
                installFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync($"GameInstallFolder_{gameIdentifier}");

            if (installFolder is null)
            {
                Debug.WriteLine("Folder instalacyjny nie został odnaleziony.");
                return;
            }

            await shellPage?.AddDownload(gameName, gameIdentifier, gameZipLink, installFolder, GameLogo.Url);

            string installPath = Path.Combine(installFolder.Path, gameIdentifier);
            await SetLocalVersion(gameIdentifier, gameVersion, gameExe, installPath);
        }
        private static string GetLocalVersion(string gameIdentifier)
        {
            using var db = new SQLiteConnection(DatabasePath);
            var game = db.Table<GameVersion>().FirstOrDefault(g => g.Identifier == gameIdentifier);
            return game?.Version ?? "0.0.0";
        }
        private static async Task SetLocalVersion(string gameIdentifier, string version, string gameExe, string path)
        {
            try
            {
                using var db = new SQLiteConnection(DatabasePath);
                db.CreateTable<GameVersion>();

                var existingGame = db.Table<GameVersion>().FirstOrDefault(g => g.Identifier == gameIdentifier);
                if (existingGame != null)
                {
                    existingGame.Path = path;
                    existingGame.Version = version;
                    existingGame.Exe = gameExe;
                    db.Update(existingGame);
                }
                else
                {
                    db.Insert(new GameVersion
                    {
                        Path = path,
                        Identifier = gameIdentifier,
                        Version = version,
                        Exe = gameExe
                    });

                }
            }
            catch (Exception ex)
            {
                await Log.ErrorAsync("Błąd zapisu wersji gry w bazie", ex);
            }
        }

        public void UninstallGame()
        {
            string path;
            bool dbEmpty = false;

            using (var db = new SQLiteConnection(DatabasePath))
            {
                var game = db.Table<GameVersion>().FirstOrDefault(g => g.Identifier == gameIdentifier);
                path = game?.Path ?? string.Empty;

                if (game != null)
                    db.Delete(game);

                dbEmpty = !db.Table<GameVersion>().Any();
            }

            try
            {
                if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                    Directory.Delete(path, true);

                if (dbEmpty && File.Exists(DatabasePath))
                    File.Delete(DatabasePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd podczas odinstalowywania gry: {ex.Message}");
            }
        }

        #region UI
        public async Task LoadLogoFromNextcloud(DataRow dr, string username, string password)
        {
            if (dr == null) return;
            string? urlObj = dr.TryGetValue("Url");
            if (string.IsNullOrWhiteSpace(urlObj)) return;
            string imageUrl = urlObj;

            try
            {
                var authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

                using var response = await httpClient.GetAsync(imageUrl);
                if (!response.IsSuccessStatusCode)
                {
                    var fallbackImage = new GameImage(imageUrl);
                    GameLogos.Add(fallbackImage);
                    GameLogo = fallbackImage;
                    return;
                }

                using var imageStream = await response.Content.ReadAsStreamAsync();
                BitmapImage bitmapImage = new BitmapImage
                {
                    DecodePixelWidth = 200
                };
                await bitmapImage.SetSourceAsync(imageStream.AsRandomAccessStream());

                GameImage gameImage = new GameImage(imageUrl, bitmapImage);
                GameLogos.Add(gameImage);
                GameLogo = gameImage;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd pobierania loga: {ex.Message}");
                var fallbackImage = new GameImage(imageUrl);
                GameLogos.Add(fallbackImage);
                GameLogo = fallbackImage;
            }
        }
        public async Task LoadImageFromNextcloud(DataRow imageRow, string username, string password)
        {
            string imageUrl = imageRow["Url"].ToString();

            using (var client = new HttpClient())
            {
                var authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

                var response = await client.GetAsync(imageUrl);
                if (response.IsSuccessStatusCode)
                {
                    var imageStream = await response.Content.ReadAsStreamAsync();
                    BitmapImage bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(imageStream.AsRandomAccessStream());

                    GameImages.Add(new GameImage(imageUrl, bitmapImage));
                }
                else
                {
                    GameImages.Add(new GameImage(imageUrl));
                }
            }
        }
        #endregion

        public void PlayGame()
        {
            // Pobranie informacji o grze z bazy
            GameVersion game;
            using (var db2 = new SQLiteConnection(DatabasePath))
            {
                game = db2.Table<GameVersion>().FirstOrDefault(g => g.Identifier == gameIdentifier);
            }

            if (game == null || string.IsNullOrWhiteSpace(game.Path) || string.IsNullOrWhiteSpace(game.Exe))
            {
                Console.WriteLine("Nie można odnaleźć informacji o grze lub pliku exe.");
                return;
            }

            string exePath;
            try
            {
                var exeFiles = Directory.GetFiles(game.Path, game.Exe + ".exe", SearchOption.AllDirectories);
                if (exeFiles.Length == 0)
                {
                    Console.WriteLine("Plik exe nie został znaleziony.");
                    return;
                }
                exePath = exeFiles[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd podczas przeszukiwania folderu: " + ex.Message);
                return;
            }

            DateTime startTime = DateTime.Now;

            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = true
                });

                process?.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd podczas uruchamiania gry: " + ex.Message);
                return;
            }

            TimeSpan elapsedTime = DateTime.Now - startTime;

            int? dbTime = db.con.scalar<int>(@"
select
  ugt.TotalTime
from UsersGameTime ugt
where ugt.GameId = @p0 and ugt.UserId = @p1", gameId, _appUser.GetCurrentUser().Id) ?? -1;

            _appUser.RegisterGamePlayed(gameId.ToInt32(), elapsedTime.TotalSeconds.ToInt32());
            GameTime = FormatPlayTime((int)dbTime);


            CoreApplication.Exit();

        }

        public static string FormatPlayTime(int totalSeconds)
        {
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;

            if (hours > 0)
            {
                return $"{hours}h {minutes}m";
            }
            else if (minutes > 0)
            {
                return $"{minutes}m {seconds}s";
            }
            else
            {
                return $"{seconds}s";
            }
        }


        #region User Ratings
        public bool UserRatingP()
        {
            DataSet ds = db.con.select(@"
select
  gr.Id
from GameRatings gr
where gr.UserId = @p0
", _appUser.GetCurrentUser().Id);

            if (ds.Tables[0].Rows.Count > 0)
                return true;
            else
                return false;
        }
        public void LoadRatings(string gameIdentifier)
        {
            Ratings.Clear();

            DataSet ds = db.con.select(@"
select
  gr.Id
, u.Id  [UserId]
, u.DisplayName
, gr.Rating
, gr.Title
, gr.Review
from GameRatings gr
inner join Games g on g.Id = gr.GameId
inner join Users u on u.Id = gr.UserId
where g.Symbol = @p0
", gameIdentifier);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                Ratings.Add(new GameRating
                {
                    RatingId = dr.TryGetValue("Id"),
                    UserId = dr.TryGetValue("UserId"),
                    Author = dr.TryGetValue("DisplayName"),
                    Rating = dr.TryGetValue("Rating"),
                    Title = dr.TryGetValue("Title"),
                    Review = dr.TryGetValue("Review")
                });
            }

            RatingsMinimal.Clear();
            foreach (var r in Ratings.Take(2))
                RatingsMinimal.Add(r);

            OnPropertyChanged(nameof(RatingsMinimal));
        }
        public void LoadGameRatingsStats(string gameIdentifier)
        {
            DataSet ds = db.con.select(@"
select 
  COUNT(gr.Id) RatingCount
, ROUND(AVG(CAST(gr.Rating AS decimal(10,2))), 2) AvgRating
from GameRatings gr
inner join Games g on g.Id = gr.GameId
where g.Symbol = @p0
group by gr.GameId
", gameIdentifier);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                RatingCount = dr.TryGetValue("RatingCount");
                AvgRating = dr.TryGetValue<double>("AvgRating");
            }

            if (AvgRating == 0)
            {
                IsRatingVisible = false;
                AvgRating = 5;
            }
        }
        public DataSet ReturnUserRating(int gameId)
        {
            return db.con.select(@"
select
  gr.Id
, u.Id [UserId]
, u.DisplayName
, gr.Rating
, gr.Title
, gr.Review
from GameRatings gr
inner join Games g on g.Id = gr.GameId
inner join Users u on u.Id = gr.UserId
where g.Id = @p0 and gr.UserId = @p1
", gameId, _appUser.GetCurrentUser().Id);
        }
        public void SaveGameRating(int gameId, GameRating gameRating)
        {
            if (gameRating.RatingId > 0)
                UpdateRating(gameRating, gameIdentifier);
            else
                db.con.exec(@"
insert GameRatings (GameId, UserId, Rating, Title, Review, CreationDateTime, ModificationDateTime)
select
  @p0
, @p1
, @p2
, @p3
, @p4
, GETDATE()
, GETDATE()
", gameId, _appUser.UserId, gameRating.Rating, gameRating.Title, gameRating.Review);

            LoadGameRatingsStats(gameIdentifier);
        }
        public void UpdateRating(GameRating gameRating, string gameIdentifier)
        {
            db.con.exec(@"
update r set
  r.Rating = @p1
, r.Title = @p2
, r.Review = @p3
, r.ModificationDateTime = GETDATE()
from GameRatings r
where r.Id = @p0
", gameRating.RatingId, gameRating.Rating, gameRating.Title, gameRating.Review);
            LoadGameRatingsStats(gameIdentifier);
        }
        #endregion
        #endregion
    }
}