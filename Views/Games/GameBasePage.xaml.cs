using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Xml.Linq;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SevenZipExtractor;
using SGSClient.Controllers;
using SGSClient.Core.Database;
using SGSClient.Core.Extensions;
using SGSClient.Helpers;
using SGSClient.Models;
using SGSClient.Services;
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
    private int? gameId;
    private string? versionFile;
    private string? gameZip;
    private string? gameExe;
    private string? gameIdentifier;
    private string? gameZipLink;

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
        base.OnNavigatedTo(e);
        DataContext = ViewModel;

        if (e.Parameter is string gameSymbol && !string.IsNullOrWhiteSpace(gameSymbol))
        {
            ViewModel.InitializeGame(gameSymbol);
        }
    }

    public GameBasePage()
    {
        ViewModel = App.GetService<GameBaseViewModel>();
        InitializeComponent();
        Status = LauncherStatus.pageLauched;
    }

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

    private async void DownloadButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        string downloadUrl = "https://sgsclient.m455yn.dev/api/shares/AutiBattlerGra/files/a0b33dc1-4cde-42a1-91b7-16108b3375e1";
        string destinationPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "game.zip");

        // Znajdź `ShellPage`
        var shellPage = (ShellPage)App.MainWindow.Content;

        await DownloadManager.Instance.StartDownloadAsync(shellPage, "Black white jump", downloadUrl, destinationPath);
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

                DateTime startTime = DateTime.Now;
                Process process = Process.Start(startInfo);
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

                MessageBox.Show($"Łączny czas działania: {TimeSpan.FromSeconds(totalTime)}");

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
