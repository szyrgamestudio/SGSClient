using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Xml.Linq;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using SevenZipExtractor;
using SGSClient.Controllers;
using SGSClient.Core.Database;
using SGSClient.Core.Extensions;
using SGSClient.Helpers;
using SGSClient.Models;
using SGSClient.ViewModels;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using File = System.IO.File;

namespace SGSClient.Views;

public sealed partial class GameBasePage : Page
{
    private LauncherStatus _status;
    private string? rootPath;
    private string? gamepath;
    private string? versionFile;
    private string? gameZip;
    private string? gameExe;
    private string? gameIdentifier;
    private string? gameZipLink;
    private string? gameVersionLink;
    private string? gameTitle;
    private string? gameDeveloper;
    private string? gameDescription;
    private string? hardwareRequirements;
    private string? otherInformations;
    //private Comment? _selectedComment;
    private GameRating? _gameRating;
    private readonly HttpClient httpClient = new();
    public GameBaseViewModel ViewModel { get; }

    internal LauncherStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            LauncherStatusHelper.UpdateStatus(PlayButton, CheckUpdateButton, UninstallButton, DownloadProgressBorder, _status, gameZip ?? "");
        }
    }
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        DataContext = ViewModel;
        if (e.Parameter is string parameterString && !string.IsNullOrWhiteSpace(parameterString))
        {
            gameIdentifier = parameterString;

            var gamesData = LoadGamesFromDatabase(true);
            var gameData = gamesData.Find(g => g.GameSymbol == gameIdentifier);

            if (gameData != null)
            {
                gameTitle = gameData.GameTitle;
                gameZipLink = gameData.GameZipLink;
                gameVersionLink = gameData.GameVersionLink;
                gameDescription = gameData.GameDescription;
                gameDeveloper = gameData.GameDeveloper;
                hardwareRequirements = gameData.HardwareRequirements;
                otherInformations = gameData.OtherInformations;
                gameExe = gameData.GameExeName;

                LoadImagesFromDatabase(gameIdentifier);
                LoadLogoFromDatabase(gameIdentifier);

                ViewModel.LoadRatings(gameIdentifier);
                ViewModel.LoadGameRatingsStats(gameIdentifier);
            }
        }

        base.OnNavigatedTo(e);

        var location = Path.Combine(ApplicationData.Current.LocalFolder.Path, "LocalState");
        rootPath = Path.GetDirectoryName(location) ?? string.Empty;

        #region
        versionFile = Path.Combine(rootPath, "versions.xml");
        gameZip = Path.Combine(rootPath, $"{gameIdentifier}ARCHIVE");
        gameExe = Path.Combine(rootPath, gameIdentifier ?? "", $"{gameExe}.exe");
        gamepath = Path.Combine(rootPath, gameIdentifier ?? "");
        #endregion

        UpdateUI();
        IsUpdated();
    }

    #region DB Handling
    public static List<GamesViewModel> LoadGamesFromDatabase(bool bypassDraftP)
    {
        List<GamesViewModel> gamesList = [];

        DataSet ds = db.con.select(@"
select
  CAST(g.Id as nvarchar(max))       [GameId]
, g.Title
, g.Symbol   [GameSymbol]
, d.Name     [GameDeveloper]
, l.LogoPath [LogoPath]
, t.Name	 [GameType]
, g.PayloadName
, g.ExeName
, g.ZipLink
, g.VersionLink
, g.Description
, g.HardwareRequirements
, g.OtherInformation
, CAST(g.DraftP as nvarchar(max)) [DraftP]
from sgsGames g
inner join sgsDevelopers d on d.Id = g.DeveloperId
left join sgsGameLogo l on l.GameId = g.Id
left join sgsGameTypes t on t.Id = g.TypeId
where g.DraftP = 0 and @p0 = 0 or @p0 = 1
order by g.Title
", bypassDraftP);

        foreach (DataRow dr in ds.Tables[0].Rows)
        {
            GamesViewModel game = new GamesViewModel
            {
                GameId = dr.TryGetValue("GameId"),
                GameSymbol = dr.TryGetValue("GameSymbol"),
                GameTitle = dr.TryGetValue("Title"),
                GamePayloadName = dr.TryGetValue("PayloadName"),
                GameExeName = dr.TryGetValue("ExeName"),
                GameZipLink = dr.TryGetValue("ZipLink"),
                GameVersionLink = dr.TryGetValue("VersionLink"),
                GameDescription = dr.TryGetValue("Description"),
                HardwareRequirements = dr.TryGetValue("HardwareRequirements"),
                OtherInformations = dr.TryGetValue("OtherInformation"),
                GameDeveloper = dr.TryGetValue("GameDeveloper"),
                LogoPath = dr.TryGetValue("LogoPath"),
                GameType = dr.TryGetValue("GameType"),
                DraftP = dr.TryGetValue("DraftP"),
            };
            gamesList.Add(game);
        }
        return gamesList;
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
    private void LoadImagesFromDatabase(string gameName)
    {
        var gameImages = LoadGalleryImagesFromDatabase(gameName);

        if (gameImages == null || gameImages.Count == 0)
        {
            Debug.WriteLine("Brak obrazów galerii w bazie danych dla tej gry.");
            return;
        }

        var flipView = FindName("GameGallery") as FlipView;
        if (flipView == null)
            return;

        flipView.Items.Clear();

        foreach (var imagePath in gameImages)
        {
            try
            {
                Uri imageUri = new Uri(imagePath);
                Image image = new Image { Source = new BitmapImage(imageUri) };
                flipView.Items.Add(image);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Błąd wczytywania obrazu: " + ex.Message);
            }
        }
    }
    private void LoadLogoFromDatabase(string gameName)
    {
        var games = LoadGamesFromDatabase(true);
        var gameData = games.Find(g => g.GameSymbol == gameName);

        if (gameData == null || string.IsNullOrEmpty(gameData.LogoPath))
        {
            SetDefaultLogoImage();
            return;
        }

        try
        {
            Uri logoImageUri = new Uri(gameData.LogoPath);
            Image? gameLogoImage = FindName("GameLogoImage") as Image;
            if (gameLogoImage != null)
                gameLogoImage.Source = new BitmapImage(logoImageUri);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Błąd wczytywania obrazu: " + ex.Message);
            SetDefaultLogoImage();
        }
    }
    #endregion

    #region UI Handling
    private void SetDefaultLogoImage()
    {
        Image? gameLogoImage = FindName("GameLogoImage") as Image;
        if (gameLogoImage != null)
            gameLogoImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/placeholder.png"));
    }
    private void UpdateUI()
    {
        GameNameTextBlock.Text = gameTitle ?? "Brak dostępnych informacji.";
        GameDeveloperTextBlock.Text = gameDeveloper ?? "Brak dostępnych informacji.";
        GameDescriptionTextBlock.Text = gameDescription ?? "Brak dostępnych informacji.";
        HardwareRequirementsTextBlock.Text = hardwareRequirements ?? "Brak dostępnych informacji.";

        if (string.IsNullOrWhiteSpace(otherInformations))
        {
            OtherInformationsTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            OtherInformationsStackPanel.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }
        else
        {
            OtherInformationsTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            OtherInformationsTextBlock.Text = otherInformations;
        }

        if (string.IsNullOrWhiteSpace(HardwareRequirementsTextBlock.Text))
        {
            reqStackPanel.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }
        else
        {
            reqStackPanel.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        }
    }
    #endregion

    #region Rating
    private void RatingRatingControl_ValueChanged(RatingControl sender, object args)
    {
        ArgumentNullException.ThrowIfNull(sender);
    }
    private async void AddRatingButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        bool hasUserRated = ViewModel.UserRatingP();

        if (hasUserRated)
        {
            List<GameRating> gameRatings = [];
            DataSet ds = ViewModel.ReturnUserRating(gameIdentifier);
            if (ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    _gameRating = new GameRating
                    {
                        RatingId = row.Field<int>("Id")
                    };
                    RatingTitleTextBox.Text = row.Field<string>("Title");
                    RatingRatingControl.Value = row.Field<int>("Rating");
                    RatingReviewTextBox.Text = row.Field<string>("Review");
                    AddRatingDetailsDialog.Title = "Oceń";
                }
            }
            AddRatingDetailsDialog.Title = "Oceń";
            await AddRatingDetailsDialog.ShowAsync();
        }
        else
        {
            _gameRating = null;
            RatingTitleTextBox.Text = string.Empty;
            RatingRatingControl.Value = 5;
            RatingReviewTextBox.Text = string.Empty;
            AddRatingDetailsDialog.Title = "Oceń";
            await AddRatingDetailsDialog.ShowAsync();
        }
    }
    private void AddRatingButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        GameRating gameRating = new();
        _gameRating = gameRating;
        var dataSet = ViewModel.ReturnUserRating(gameIdentifier);
        if (dataSet.Tables[0].Rows.Count > 0)
        {
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                _gameRating.RatingId = row.Field<int>("Id");
            }
        }
        _gameRating.Title = RatingTitleTextBox.Text;
        _gameRating.Rating = (int)RatingRatingControl.Value;
        _gameRating.Review = RatingReviewTextBox.Text;
        ViewModel.AddRating(gameIdentifier, _gameRating);
        ViewModel.LoadRatings(gameIdentifier);

        AddRatingDetailsDialog.Hide();
    }
    private void ShowAllReviewsButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {

    }
    #endregion

    public GameBasePage()
    {
        ViewModel = App.GetService<GameBaseViewModel>();
        InitializeComponent();
        Status = LauncherStatus.pageLauched;
    }

    private void IsUpdated()
    {
        try
        {
            SGSVersion.Version localVersion = GetLocalVersion();
            SGSVersion.Version onlineVersion = GetOnlineVersion();

            Status = (localVersion.ToString() == "0.0.0") ? LauncherStatus.readyNoGame : LauncherStatus.ready;

            if (onlineVersion.IsDifferentThan(localVersion) && localVersion.ToString() != "0.0.0")
                CheckUpdateButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
            Status = LauncherStatus.failed;
        }
    }
    private SGSVersion.Version GetLocalVersion()
    {
        XDocument versionXml;

        if (File.Exists(Path.Combine(rootPath ?? "", "versions.xml")) && (versionXml = XDocument.Load(Path.Combine(rootPath ?? "", "versions.xml"))) != null)
        {
            XElement? gameVersionElement = versionXml.Root?.Element(gameIdentifier);
            return gameVersionElement != null ? new SGSVersion.Version(gameVersionElement.Value) : new SGSVersion.Version("0.0.0.0");
        }
        else
        {
            return new SGSVersion.Version("0.0.0.0");
        }
    }
    private SGSVersion.Version GetOnlineVersion()
    {
        try
        {
            var onlineVersionString = GetGameVersion(gameIdentifier);
            return new SGSVersion.Version(onlineVersionString);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Błąd podczas pobierania wersji gry z bazy danych: {ex.Message}");
            throw;
        }
    }
    public string GetGameVersion(string gameIdentifier)
    {
        List<string> galleryImages = [];
        var ds = db.con.select(@"
select
  g.CurrentVersion
from sgsGames g
where g.Symbol = @p0
", gameIdentifier);
        if (ds.Tables[0].Rows.Count > 0)
            return ds.Tables[0].Rows[0].TryGetValue("CurrentVersion");
        else
            return "0.0.0.0";
    }

    private void CheckForUpdates()
    {
        try
        {
            SGSVersion.Version localVersion = GetLocalVersion();
            SGSVersion.Version onlineVersion = GetOnlineVersion();

            if (onlineVersion.IsDifferentThan(localVersion))
            {
                Status = LauncherStatus.downloadingUpdate;
                InstallGameFiles(true, onlineVersion);
            }
            else
            {
                Status = LauncherStatus.ready;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
            Status = LauncherStatus.failed;
        }
    }
    private async void InstallGameFiles(bool _isUpdate, SGSVersion.Version _onlineVersion)
    {
        try
        {
            if (_isUpdate)
            {
                Status = LauncherStatus.downloadingUpdate;
            }
            else
            {
                Status = LauncherStatus.downloadingGame;
                _onlineVersion = GetOnlineVersion();
            }

            HttpResponseMessage response = await httpClient.GetAsync(new Uri(gameZipLink ?? ""));
            response.EnsureSuccessStatusCode();

            using (Stream contentStream = await response.Content.ReadAsStreamAsync())
            {
                if (!string.IsNullOrEmpty(gameZip) && !string.IsNullOrEmpty(rootPath))
                {
                    using FileStream fileStream = new(gameZip, FileMode.Create, FileAccess.Write, FileShare.None);
                    await contentStream.CopyToAsync(fileStream);
                }
                else
                {
                    Status = LauncherStatus.failed;
                    return;
                }
            }

            if (!string.IsNullOrEmpty(gameZip) && !string.IsNullOrEmpty(rootPath))
            {
                using (ArchiveFile archiveFile = new ArchiveFile(Path.Combine(rootPath, gameZip)))
                {
                    archiveFile.Extract(rootPath);
                }
                File.Delete(gameZip);
            }
            else
            {
                Status = LauncherStatus.failed;
                return;
            }

            if (!string.IsNullOrEmpty(rootPath))
            {
                XDocument versionXml;

                var versionXmlPath = Path.Combine(rootPath, "versions.xml");
                versionXml = File.Exists(versionXmlPath) ? XDocument.Load(versionXmlPath) : new XDocument(new XElement("Versions"));

                XElement? gameVersionElement = versionXml.Root?.Element(gameIdentifier);
                if (gameVersionElement != null)
                {
                    gameVersionElement.Value = _onlineVersion.ToString();
                }
                else
                {
                    versionXml.Root?.Add(new XElement(gameIdentifier, _onlineVersion.ToString()));
                }

                versionXml.Save(versionXmlPath);
            }
            else
            {
                Status = LauncherStatus.failed;
                return;
            }

            Status = LauncherStatus.ready;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
            Status = LauncherStatus.failed;
        }
    }

    #region Buttons
    private void PlayClickButton(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (File.Exists(gameExe))
        {
            try
            {
                ProcessStartInfo startInfo = new(gameExe)
                {
                    WorkingDirectory = Path.Combine(rootPath ?? "")
                };
                Process.Start(startInfo);
                CoreApplication.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        else
        {
            try
            {
                CheckForUpdates();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
    private void UninstallClickButton(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (Directory.Exists(gamepath))
        {
            uninstallFlyout.Hide();
            Directory.Delete(gamepath, true);

            if (File.Exists(Path.Combine(rootPath ?? "", "versions.xml")))
            {
                XDocument versionXml = XDocument.Load(Path.Combine(rootPath ?? "", "versions.xml"));
                XElement? gameVersionElement = versionXml.Root?.Element(gameIdentifier);

                if (gameVersionElement != null)
                {
                    gameVersionElement.Remove();
                    versionXml.Save(Path.Combine(rootPath ?? "", "versions.xml"));
                }
            }

            File.Delete(gameZip ?? "");
            Status = LauncherStatus.readyNoGame;
        }
    }
    private void UpdateClickButton(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        CheckForUpdates();
    }
    #endregion


}
