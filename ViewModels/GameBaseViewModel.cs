using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;
using SGSClient.Core.Authorization;
using SGSClient.Core.Database;
using SGSClient.Core.Extensions;
using SGSClient.Core.Utilities.AppInfoUtility.Interfaces;
using SGSClient.Core.Utilities.LogUtility;
using SGSClient.Models;
using SGSClient.Views;
using SQLite;
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
        private ObservableCollection<GameRating> _allRatings;
        private const int PageSize = 2;
        [ObservableProperty]
        private ObservableCollection<GameRating> ratings;
        [ObservableProperty]
        private int currentPage;
        [ObservableProperty]
        private int ratingCount;
        [ObservableProperty]
        private string avgRating;
        [ObservableProperty]
        private int count1;
        [ObservableProperty]
        private int count2;
        [ObservableProperty]
        private int count3;
        [ObservableProperty]
        private int count4;
        [ObservableProperty]
        private int count5;
        private string? rootPath;
        private string? gamepath;
        private int? gameId;
        private string? versionFile;
        private string? gameZip;
        private string? gameExe;
        private string? gameIdentifier;
        private string? gameName;
        private string? gameZipLink;
        private string gameVersion;
        private readonly HttpClient httpClient = new();
        private static readonly string DatabasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "local_game_data.db");

        public int? GameId { get; private set; }
        public string? GameName { get; private set; }
        public string? GameDeveloper { get; private set; }
        public string? GameDescription { get; private set; }
        public string? HardwareRequirements { get; private set; }
        public string? OtherInformations { get; private set; }
        public bool IsOtherInformationsVisible { get; private set; }
        public bool IsHardwareRequirementsVisible { get; private set; }
        public bool IsDLCVisible { get; private set; }
        public bool IsAddRatingVisible { get; private set; }
        public bool CanGoToPreviousPage => CurrentPage > 0;
        public bool CanGoToNextPage => (CurrentPage + 1) * PageSize < _allRatings.Count;

        #region Logo
        private GameImage _gameLogo;
        public GameImage GameLogo
        {
            get => _gameLogo;
            set
            {
                _gameLogo = value;
                OnPropertyChanged(nameof(GameLogo));
            }
        }
        public ObservableCollection<GameImage> GameLogos { get; set; } = new();

        #endregion

        #region Images gallery
        private ObservableCollection<GameImage> _gameImages;
        public ObservableCollection<GameImage> GameImages
        {
            get => _gameImages;
            set
            {
                _gameImages = value;
                OnPropertyChanged(nameof(GameImages));
            }
        }
        public ObservableCollection<string> GameImagePaths { get; set; } = new ObservableCollection<string>();
        #endregion

        #endregion

        public GameBaseViewModel(IAppUser appUser, IAppInfo appInfo)
        {
            _appUser = appUser;
            _appInfo = appInfo;

            _allRatings = [];
            ratingCount = 0;
            avgRating = "5.0";
            count1 = 0;
            count2 = 0;
            count3 = 0;
            count4 = 0;
            count5 = 0;


            CurrentPage = 0;

            GameLogos = [];
            GameImages = [];
            Ratings = [];
        }

        public async Task LoadGameData(string gameSymbol)
        {
            string nextcloudLogin = _appInfo.GetAppSetting("NextcloudLogin").Value;
            string nextcloudPassword = _appInfo.GetAppSetting("NextcloudPassword").Value;

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

                //LoadRatings(gameSymbol ?? "");
                //LoadGameRatingsStats(gameSymbol ?? "");

                GameId = gameId;
                GameName = gameName;
                GameDeveloper = dr.TryGetValue("GameDeveloper") ?? "Brak dostępnych informacji.";
                GameDescription = dr.TryGetValue("Description") ?? "Brak dostępnych informacji.";
                HardwareRequirements = dr.TryGetValue("HardwareRequirements") ?? "Brak dostępnych informacji.";
                OtherInformations = dr.TryGetValue("OtherInformation");
                IsOtherInformationsVisible = !String.IsNullOrEmpty(dr.TryGetValue("OtherInformation"));
                IsHardwareRequirementsVisible = !String.IsNullOrEmpty(dr.TryGetValue("HardwareRequirements"));
                IsDLCVisible = false; //TODO
                IsAddRatingVisible = _appUser.GetCurrentUser().Id != default;

                OnPropertyChanged(nameof(GameId));
                OnPropertyChanged(nameof(GameName));
                OnPropertyChanged(nameof(GameDeveloper));
                OnPropertyChanged(nameof(GameDescription));
                OnPropertyChanged(nameof(HardwareRequirements));
                OnPropertyChanged(nameof(OtherInformations));
                OnPropertyChanged(nameof(IsOtherInformationsVisible));
                OnPropertyChanged(nameof(IsHardwareRequirementsVisible));
                OnPropertyChanged(nameof(IsDLCVisible));
                OnPropertyChanged(nameof(IsAddRatingVisible));

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

            if (StorageApplicationPermissions.FutureAccessList.ContainsItem("GameInstallFolder"))
                installFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("GameInstallFolder");

            if (installFolder is null)
            {
                Debug.WriteLine("Folder instalacyjny nie został odnaleziony.");
                return;
            }

            shellPage?.AddDownload(gameName, gameIdentifier, gameZipLink, installFolder, GameLogo.Url);

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
            using var db = new SQLiteConnection(DatabasePath);
            var game = db.Table<GameVersion>().FirstOrDefault(g => g.Identifier == gameIdentifier);
            string path = game?.Path ?? string.Empty;

            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);

                if (!db.Table<GameVersion>().Any())
                    if (File.Exists(DatabasePath))
                        File.Delete(DatabasePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd podczas odinstalowywania gry: {ex.Message}");
            }
        }

        #region UI
        public async Task LoadLogoFromNextcloud(DataRow imageRow, string username, string password)
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

                    GameImage gameImage = new GameImage(imageUrl, bitmapImage);
                    GameLogo = gameImage;
                    GameLogos.Add(gameImage);
                }
                else
                {
                    GameImage gameImage = new GameImage(imageUrl);
                    GameLogos.Add(gameImage);
                    GameLogo = gameImage;
                }
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

            double totalTime = (dbTime ?? 0) + elapsedTime.TotalSeconds;

            if (dbTime >= 0)
            {
                db.con.exec(@"
update ugt set 
  ugt.TotalTime = @p0
, ugt.LastPlayed = @p1
from UsersGameTime ugt
where ugt.GameId = @p2 and ugt.UserId = @p3", totalTime, DateTime.Now, gameId, _appUser.GetCurrentUser().Id);
            }
            else
            {
                db.con.exec(@"
insert UsersGameTime (UserId, GameId, LastPlayed, TotalTime)
select
  @p0
, @p1
, @p2
, @p3
", _appUser.GetCurrentUser().Id, gameId, DateTime.Now, totalTime);
            }

            CoreApplication.Exit();

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
            _allRatings.Clear();

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
                _allRatings.Add(new GameRating
                {
                    RatingId = dr.TryGetValue("Id"),
                    UserId = dr.TryGetValue("UserId"),
                    Author = dr.TryGetValue("Name"),
                    Rating = dr.TryGetValue("Rating"),
                    Title = dr.TryGetValue("Title"),
                    Review = dr.TryGetValue("Review")
                });
            }

            LoadPage(0);
        }
        public void LoadGameRatingsStats(string gameIdentifier)
        {
            DataSet ds = db.con.select(@"
select 
  COUNT(gr.Id) RatingCount
, CAST(ROUND(CAST(AVG(gr.Rating) as decimal(10, 1)), 1) as nvarchar) AvgRating
, SUM(case when gr.Rating = 1 then 1 else 0 end) * 100 / COUNT(gr.Id) Count1
, SUM(case when gr.Rating = 2 then 1 else 0 end) * 100 / COUNT(gr.Id) Count2
, SUM(case when gr.Rating = 3 then 1 else 0 end) * 100 / COUNT(gr.Id) Count3
, SUM(case when gr.Rating = 4 then 1 else 0 end) * 100 / COUNT(gr.Id) Count4
, SUM(case when gr.Rating = 5 then 1 else 0 end) * 100 / COUNT(gr.Id) Count5
from GameRatings gr
inner join Games g on g.Id = gr.GameId
where g.Symbol = @p0
group by gr.GameId
", gameIdentifier);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                RatingCount = dr.TryGetValue("RatingCount");
                AvgRating = dr.TryGetValue("AvgRating");
                Count1 = dr.TryGetValue("Count1");
                Count2 = dr.TryGetValue("Count2");
                Count3 = dr.TryGetValue("Count3");
                Count4 = dr.TryGetValue("Count4");
                Count5 = dr.TryGetValue("Count5");
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
            LoadPage(CurrentPage);
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
            LoadPage(CurrentPage);
        }
        public void LoadPage(int pageNumber)
        {
            Ratings.Clear();
            CurrentPage = pageNumber;
            var ratingsToShow = _allRatings.Skip(CurrentPage * PageSize).Take(PageSize);
            foreach (var gameRating in ratingsToShow)
            {
                Ratings.Add(gameRating);
            }
            OnPropertyChanged(nameof(CanGoToPreviousPage));
            OnPropertyChanged(nameof(CanGoToNextPage));
        }
        #endregion
    }
}