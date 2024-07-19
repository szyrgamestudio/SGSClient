using SGSClient.Controllers;
using SGSClient.Helpers;
using SGSClient.ViewModels;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using System.Windows;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using File = System.IO.File;
using Microsoft.UI.Xaml.Navigation;
using System.Xml.Linq;
using Microsoft.UI.Xaml.Media.Imaging;
using SGSClient.Core.Database;
using SevenZipExtractor;
using SGSClient.Models;

namespace SGSClient.Views;

public sealed partial class GameBasePage : Page
{
    private LauncherStatus _status;
    private string? rootPath;
    private string? gamepath;
    private string? versionFile;
    private string? gameZip;
    private string? gameExe;
    private string gameIdentifier;
    private string? gameZipLink;
    private string? gameVersionLink;
    private string? gameTitle;
    private string? gameDeveloper;
    private string? gameDescription;
    private string? hardwareRequirements;
    private string? otherInformations;
    private Comment _selectedComment;
    private readonly ConfigurationManagerSQL configManagerSQL;
    private readonly HttpClient httpClient = new();

    internal LauncherStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            LauncherStatusHelper.UpdateStatus(PlayButton, CheckUpdateButton, UninstallButton, DownloadProgressBorder, _status, gameZip ?? "");
        }
    }
    public GameBaseViewModel ViewModel
    {
        get;
    }
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is string parameterString && !string.IsNullOrWhiteSpace(parameterString))
        {
            gameIdentifier = parameterString;

            var gameData = configManagerSQL.LoadGamesFromDatabase(true).Find(g => g.GameSymbol == gameIdentifier);
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

                ViewModel.LoadComments(gameIdentifier);
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
    private void LoadImagesFromDatabase(string gameName)
    {
        var gameImages = configManagerSQL.LoadGalleryImagesFromDatabase(gameName);

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
        var gameData = configManagerSQL.LoadGamesFromDatabase(true).Find(g => g.GameSymbol == gameName);

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

    #region Comments
    private void CommentsListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        string userId = AppSession.CurrentUserSession.UserId;
        if (e.ClickedItem is Comment comment && comment.AuthorId.ToString() == userId)
        {
            _selectedComment = comment;
            AuthorTextBox.Text = comment.Author;
            ContentTextBox.Text = comment.Content;
            _ = CommentDetailsDialog.ShowAsync();
        }
    }
    private void AddCommentButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        _selectedComment = null; // Indicates that we are adding a new comment
        ACAuthorTextBox.Text = string.Empty;
        ACContentTextBox.Text = string.Empty;
        AddCommentDetailsDialog.Title = "Dodaj komentarz";
        _ = AddCommentDetailsDialog.ShowAsync();
    }

    private void SaveNewCommentButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (_selectedComment != null)
        {
            _selectedComment.Author = AuthorTextBox.Text;
            _selectedComment.Content = ContentTextBox.Text;
            ViewModel.AddComment(_selectedComment);
            ViewModel.LoadComments(gameIdentifier); // Refresh comments
        }
        CommentDetailsDialog.Hide();
    }

    private void SaveCommentButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (_selectedComment != null)
        {
            _selectedComment.Author = AuthorTextBox.Text;
            _selectedComment.Content = ContentTextBox.Text;
            ViewModel.UpdateComment(_selectedComment);
            ViewModel.LoadComments(gameIdentifier); // Refresh comments
        }
        CommentDetailsDialog.Hide();
    }

    private void DeleteCommentButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (_selectedComment != null)
        {
            ViewModel.DeleteComment(_selectedComment);
            _selectedComment = null;
            ViewModel.LoadComments(gameIdentifier); // Refresh comments
        }
    }
    #endregion

    public GameBasePage()
    {
        var ConnectionString = Db.GetConnectionString();
        configManagerSQL = new ConfigurationManagerSQL(ConnectionString);

        ViewModel = new GameBaseViewModel(new ConfigurationManagerSQL(Db.GetConnectionString()));
        InitializeComponent();
        DataContext = ViewModel;  // Set the DataContext
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
            var onlineVersionString = configManagerSQL.GetGameVersion(gameIdentifier);
            return new SGSVersion.Version(onlineVersionString);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Błąd podczas pobierania wersji gry z bazy danych: {ex.Message}");
            throw;
        }
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
