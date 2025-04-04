using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using SGSClient.Core.Authorization;
using SGSClient.Core.Database;
using SGSClient.Core.Extensions;
using SGSClient.Core.Utilities.LogUtility;
using SGSClient.DataAccess.Repositories;
using SGSClient.Models;
using SGSClient.Views;
using SQLite;
using Windows.ApplicationModel.Core;
using Windows.Storage;

namespace SGSClient.ViewModels
{
    public partial class GameBaseViewModel : ObservableRecipient
    {
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


        public ObservableCollection<BitmapImage> GameImages { get; } = new();
        public string? GameLogo { get; private set; }
        public string? GameName { get; private set; }
        public string? GameDeveloper { get; private set; }
        public string? GameDescription { get; private set; }
        public string? HardwareRequirements { get; private set; }
        public string? OtherInformations { get; private set; }
        public bool IsOtherInformationsVisible { get; private set; }
        public bool IsHardwareRequirementsVisible { get; private set; }

        public void InitializeGame(string gameSymbol)
        {
            var gamesData = GamesRepository.FetchGames(true);
            var gameData = gamesData.FirstOrDefault(g => g.GameSymbol == gameSymbol);

            if (gameData != null)
            {
                gameId = gameData.GameId;
                gameZip = gameData.GameZipLink;
                gameExe = gameData.GameExeName;
                gameName = gameData.GameName;
                gameIdentifier = gameData.GameSymbol;
                gameZipLink = gameData.GameZipLink;
                gameVersion = db.con.scalar(@"
select
  g.CurrentVersion
from sgsGames g
where g.Id = @p0
", gameId);

                LoadRatings(gameData.GameSymbol ?? "");
                LoadGameRatingsStats(gameData.GameSymbol ?? "");
                LoadImagesFromDatabase(gameData.GameSymbol ?? "");
                LoadLogoFromDatabase(gameData.GameSymbol ?? "");

                UpdateUI(gameData);
            }


            string rootPath = ApplicationData.Current.LocalFolder.Path;
            gamepath = Path.Combine(rootPath, gameIdentifier ?? "DefaultGame");

            versionFile = Path.Combine(rootPath, "versions.xml");
            gameZip = Path.Combine(rootPath, $"{gameIdentifier}ARCHIVE");
            gameExe = Path.Combine(rootPath, gameIdentifier ?? "", $"{gameExe}.exe");
        }
        public async Task DownloadGame(ShellPage shellPage, string? installPath)
        {
            if (!string.IsNullOrEmpty(gameName) && !string.IsNullOrEmpty(gameZipLink) && !string.IsNullOrEmpty(GameLogo) && !string.IsNullOrEmpty(gameExe))
            {
                shellPage?.AddDownload(gameName, gameZipLink, installPath ?? ApplicationData.Current.LocalFolder.Path, GameLogo);

                await SetLocalVersion(gameName, gameVersion, gameExe);
            }
        }
        private static async Task SetLocalVersion(string gameIdentifier, string version, string gameExe)
        {
            try
            {
                using var db = new SQLiteConnection(DatabasePath);
                db.CreateTable<GameVersion>();

                var existingGame = db.Table<GameVersion>().FirstOrDefault(g => g.Identifier == gameIdentifier);
                if (existingGame != null)
                {
                    existingGame.Version = version;
                    existingGame.Exe = gameExe;
                    db.Update(existingGame);
                }
                else
                {
                    db.Insert(new GameVersion
                    {
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
        private static string GetLocalVersion(string gameIdentifier)
        {
            using var db = new SQLiteConnection(DatabasePath);
            var game = db.Table<GameVersion>().FirstOrDefault(g => g.Identifier == gameIdentifier);
            return game?.Version ?? "0.0.0";
        }
        public (bool installedP, bool isUpdateP) CheckForUpdate()
        {
            try
            {
                string localVersion = GetLocalVersion(gameIdentifier ?? "");

                if (string.IsNullOrEmpty(localVersion))
                {
                    Debug.WriteLine("Brak zainstalowanej wersji gry.");
                    return (false, false);
                }

                bool isUpdateP = localVersion != gameVersion;
                if (isUpdateP)
                {
                    Debug.WriteLine($"Dostępna nowa wersja: {gameVersion}. Aktualna: {localVersion}");
                    //CheckUpdateButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                }
                else
                {
                    Debug.WriteLine("Gra jest aktualna.");
                }

                return (true, isUpdateP);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd podczas sprawdzania aktualizacji: {ex.Message}");
                return (false, false);
            }
        }


        #region UI
        private void UpdateUI(Game game)
        {
            GameName = game.GameName ?? "Brak dostępnych informacji.";
            GameDeveloper = game.GameDeveloper ?? "Brak dostępnych informacji.";
            GameDescription = game.GameDescription ?? "Brak dostępnych informacji.";
            HardwareRequirements = game.HardwareRequirements ?? "Brak dostępnych informacji.";
            OtherInformations = string.IsNullOrWhiteSpace(game.OtherInformations) ? null : game.OtherInformations;
            IsOtherInformationsVisible = !string.IsNullOrWhiteSpace(game.OtherInformations);
            IsHardwareRequirementsVisible = !string.IsNullOrWhiteSpace(game.HardwareRequirements);

            OnPropertyChanged(nameof(GameName));
            OnPropertyChanged(nameof(GameDeveloper));
            OnPropertyChanged(nameof(GameDescription));
            OnPropertyChanged(nameof(HardwareRequirements));
            OnPropertyChanged(nameof(OtherInformations));
            OnPropertyChanged(nameof(IsOtherInformationsVisible));
            OnPropertyChanged(nameof(IsHardwareRequirementsVisible));
        }
        public static List<string> LoadGalleryImagesFromDatabase(string gameSymbol)
        {
            List<string> galleryImages = [];
            var ds = db.con.select(@"
select
  i.ImagePath
from sgsGameImages i
inner join sgsGames g on g.Id = i.GameId
where g.Symbol = @p0
", gameSymbol);
            foreach (DataRow dr in ds.Tables[0].Rows)
                galleryImages.Add(dr.TryGetValue("ImagePath").ToString());

            return galleryImages;
        }
        public void LoadImagesFromDatabase(string gameSymbol)
        {
            GameImages.Clear();
            var gameImages = LoadGalleryImagesFromDatabase(gameSymbol);

            if (gameImages == null || gameImages.Count == 0)
            {
                Debug.WriteLine("Brak obrazów galerii w bazie danych dla tej gry.");
                return;
            }

            foreach (var imagePath in gameImages)
            {
                try
                {
                    Uri imageUri = new Uri(imagePath);
                    GameImages.Add(new BitmapImage(imageUri));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Błąd wczytywania obrazu: " + ex.Message);
                }
            }

            OnPropertyChanged(nameof(GameImages));
        }
        public void LoadLogoFromDatabase(string gameSymbol)
        {
            var games = GamesRepository.FetchGames(true);
            var gameData = games.FirstOrDefault(g => g.GameSymbol == gameSymbol);

            GameLogo = gameData?.LogoPath ?? "ms-appx:///Assets/placeholder.png";
            OnPropertyChanged(nameof(GameLogo));
        }
        #endregion

        public void PlayGame()
        {
            if (File.Exists(gameExe))
            {
                ProcessStartInfo startInfo = new(gameExe)
                {
                    WorkingDirectory = Path.Combine(rootPath ?? "")
                };

                DateTime startTime = DateTime.Now;
                System.Diagnostics.Process process = System.Diagnostics.Process.Start(startInfo);
                process.WaitForExit();

                TimeSpan elapsedTime = DateTime.Now - startTime;

                int dbTime = db.con.scalar<int>(@"
select
  ugt.TotalTime
from sgsUsersGameTime ugt
inner join Registration r on r.Id = ugt.UserId
where ugt.GameId = @p0 and r.UserId = @p1", gameId, "a8fcd7ae-2410-46f4-9943-fc790905d9bb") ?? -1;

                double totalTime = 0;
                totalTime = Convert.ToDouble(dbTime < 0 ? 0 : dbTime);
                totalTime += elapsedTime.TotalSeconds;

                if (dbTime >= 0)
                {
                    db.con.exec(@"
update ugt set 
  ugt.TotalTime = @p0
, ugt.LastPlayed = @p1
from sgsUsersGameTime ugt
inner join Registration r on r.Id = ugt.UserId
where ugt.GameId = @p2 and r.UserId = @p3", totalTime, DateTime.Now, gameId, "a8fcd7ae-2410-46f4-9943-fc790905d9bb");
                }
                else
                {
                    db.con.exec(@"
declare @userId int = 
(
  select
    r.Id
  from Registration r
  where r.UserId = @p0
)

insert sgsUsersGameTime (UserId, GameId, LastPlayed, TotalTime)
select
  @userId
, @p1
, @p2
, @p3
", "a8fcd7ae-2410-46f4-9943-fc790905d9bb", gameId, DateTime.Now, totalTime);
                }

                //MessageBox.Show($"Łączny czas działania: {TimeSpan.FromSeconds(totalTime)}");

                CoreApplication.Exit();

            }
            else
            {
                Debug.WriteLine("Brak pliku exe gry.");
            }
        }

        #region Constructors and Properties
        private readonly IAppUser _appUser;
        private ObservableCollection<GameRating> _allRatings;
        private const int PageSize = 2;


        public bool CanGoToPreviousPage => CurrentPage > 0;
        public bool CanGoToNextPage => (CurrentPage + 1) * PageSize < _allRatings.Count;

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

        public GameBaseViewModel(IAppUser appUser)
        {
            ratingCount = 0;
            avgRating = "5.0";
            count1 = 0;
            count2 = 0;
            count3 = 0;
            count4 = 0;
            count5 = 0;

            _allRatings = new ObservableCollection<GameRating>();

            Ratings = new ObservableCollection<GameRating>();
            CurrentPage = 0;
            _appUser = appUser;
        }

        #endregion
        public bool UserRatingP()
        {
            DataSet ds = db.con.select(@"
select
  gr.Id
from GameRatings gr
inner join Registration r on r.Id = gr.UserId
where r.UserId = @p0
", _appUser.UserId);

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
, d.Id [DeveloperId]
, d.Name
, gr.Rating
, gr.Title
, gr.Review
from GameRatings gr
inner join sgsGames g on g.Id = gr.GameId
inner join Registration r on r.Id = gr.UserId
inner join sgsDevelopers d on d.Id = r.DeveloperId
where g.Symbol = @p0
", gameIdentifier);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                _allRatings.Add(new GameRating
                {
                    RatingId = dr.TryGetValue("Id"),
                    UserId = dr.TryGetValue("DeveloperId"),
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
, CAST(ROUND(CAST(AVG(gr.Rating) AS DECIMAL(10, 1)), 1) as nvarchar) AvgRating
, SUM(CASE WHEN gr.Rating = 1 THEN 1 ELSE 0 END) * 100 / COUNT(gr.Id) Count1
, SUM(CASE WHEN gr.Rating = 2 THEN 1 ELSE 0 END) * 100 / COUNT(gr.Id) Count2
, SUM(CASE WHEN gr.Rating = 3 THEN 1 ELSE 0 END) * 100 / COUNT(gr.Id) Count3
, SUM(CASE WHEN gr.Rating = 4 THEN 1 ELSE 0 END) * 100 / COUNT(gr.Id) Count4
, SUM(CASE WHEN gr.Rating = 5 THEN 1 ELSE 0 END) * 100 / COUNT(gr.Id) Count5
from GameRatings gr
inner join sgsGames g on g.Id = gr.GameId
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
        public DataSet ReturnUserRating(string gameIdentifier)
        {
            return db.con.select(@"
select
  gr.Id
, d.Id [DeveloperId]
, d.Name
, gr.Rating
, gr.Title
, gr.Review
from GameRatings gr
inner join sgsGames g on g.Id = gr.GameId
inner join Registration r on r.Id = gr.UserId
inner join sgsDevelopers d on d.Id = r.DeveloperId
where g.Symbol = @p0 and r.UserId = @p1
", gameIdentifier, _appUser.UserId);
        }

        public void AddRating(string gameIdentifier, GameRating gameRating)
        {
            if (gameRating.RatingId > 0)
                UpdateRating(gameRating, gameIdentifier);
            else
                db.con.exec(@"
declare @gameId int =
(
  select
    g.Id
  from sgsGames g
  where g.Symbol = @p0
)

declare @userId int = 
(
  select
    r.Id
  from Registration r
  where  r.UserId = @p1
)

insert into GameRatings (GameId, UserId, Rating, Title, Review, CreationDateTime, ModificationDateTime)
select
  @gameId
, @userId
, @p2
, @p3
, @p4
, GETDATE()
, GETDATE()
", gameIdentifier, _appUser.UserId, gameRating.Rating, gameRating.Title, gameRating.Review);

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
    }
}