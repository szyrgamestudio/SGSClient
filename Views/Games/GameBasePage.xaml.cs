using SGSClient.Contracts.Services;
using SGSClient.Controllers;
using SGSClient.Helpers;
using SGSClient.ViewModels;
using Microsoft.UI.Xaml.Controls;
using System.IO.Compression;
using System.Diagnostics;
using System.Windows;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using File = System.IO.File;
using Microsoft.UI.Xaml.Navigation;
using System.Xml.Linq;
using Microsoft.UI.Xaml.Media.Imaging;

namespace SGSClient.Views
{
    public sealed partial class GameBasePage : Page
    {
        private string? rootPath;
        private string? gamepath;
        private string? versionFile;
        private string? gameZip;
        private string? gameExe;
        private LauncherStatus _status;

        /*From appConfig.xml*/
        private string? gameIdentifier;
        private string? gameZipLink;
        private string? gameVersionLink;
        private string? gameTitle;
        private string? gameDeveloper;
        private string? gamePayloadName;
        private string? gameDescription;
        private string? hardwareRequirements;
        private string? otherInformations;

        private readonly HttpClient httpClient = new();
        ConfigurationManager configManager = new ConfigurationManager("C:\\Users\\mafelt\\source\\repos\\SGSClient\\Config\\appconfig.xml");
        internal LauncherStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                LauncherStatusHelper.UpdateStatus(PlayButton, CheckUpdateButton, UninstallButton, DownloadProgressBorder, _status, gameZip ?? "");
            }
        }
        public GameBaseViewModel ViewModel { get; }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is string parameterString && !string.IsNullOrWhiteSpace(parameterString))
            {
                gameIdentifier = parameterString;

                gameTitle = configManager.GetGameTitle(gameIdentifier);
                gameZipLink = configManager.GetGameZipLink(gameIdentifier);
                gameVersionLink = configManager.GetGameVersionLink(gameIdentifier);
                gamePayloadName = configManager.GetGamePayloadName(gameIdentifier);
                gameDescription = configManager.GetGameDescription(gameIdentifier);
                gameDeveloper = configManager.GetGameDeveloper(gameIdentifier);
                hardwareRequirements = configManager.GetHardwareRequirements(gameIdentifier);
                otherInformations = configManager.GetOtherInformations(gameIdentifier);

                LoadImagesFromXml(gameIdentifier);
                LoadLogoFromXml(gameIdentifier);
            }

            base.OnNavigatedTo(e);

            string location = Path.Combine(ApplicationData.Current.LocalFolder.Path, "LocalState");
            rootPath = Path.GetDirectoryName(location) ?? string.Empty;

            versionFile = Path.Combine(rootPath, "versionZACMIENIE.txt"); //docelowo lokalny xml, gdzie trzymać bedziemy jaką wersje gry mamy
            gameZip = Path.Combine(rootPath, $"{gameIdentifier}.zip");
            gameExe = Path.Combine(rootPath, gameIdentifier ?? "", $"{gameIdentifier}.exe");
            gamepath = Path.Combine(rootPath, gameIdentifier ?? "");
            UpdateUI();
            IsUpdated();
        }
        private void LoadImagesFromXml(string gameName)
        {
            XElement? gameElement = configManager.GetGameElement(gameName);

            if (gameElement != null)
            {
                FlipView? flipView = FindName("GameGallery") as FlipView;

                if (flipView != null)
                {
                    flipView.Items.Clear();
                    var galleryImagesElement = gameElement.Element("GalleryImages");

                    if (galleryImagesElement != null)
                    {
                        var imageElements = galleryImagesElement.Elements("GalleryImage");

                        foreach (var imageElement in imageElements)
                        {
                            string imagePath = imageElement.Value;
                            Uri imageUri = new Uri("ms-appx:///" + imagePath);
                            Image image = new Image { Source = new BitmapImage(imageUri) };

                            flipView.Items.Add(image);
                        }
                    }
                }
            }
        }
        private void LoadLogoFromXml(string gameName)
        {
            XElement? gameElement = configManager.GetGameElement(gameName);

            if (gameElement != null)
            {
                Image? gameLogoImage = FindName("GameLogoImage") as Image;

                if (gameLogoImage != null)
                {
                    // Pobierz wszystkie obrazy z elementu XML
                    var logoImagesElement = gameElement.Element("LogoImages");

                    if (logoImagesElement != null)
                    {
                        var logoImageElement = logoImagesElement.Element("LogoImage");

                        if (logoImageElement != null)
                        {
                            string logoImagePath = logoImageElement.Value;

                            try
                            {
                                // Utwórz bezwzględny identyfikator URI
                                Uri logoImageUri = new Uri("ms-appx:///" + logoImagePath);

                                // Ustaw źródło obrazu
                                gameLogoImage.Source = new BitmapImage(logoImageUri);
                            }
                            catch (Exception ex)
                            {
                                // Błąd wczytywania obrazu - użyj obrazka zastępczego
                                gameLogoImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/placeholder.png"));
                                Debug.WriteLine("Błąd wczytywania obrazu: " + ex.Message);
                            }
                        }
                        else
                        {
                            // Brak elementu LogoImage w XML - użyj obrazka zastępczego
                            gameLogoImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/placeholder.png"));
                        }
                    }
                    else
                    {
                        // Brak elementu LogoImages w XML - użyj obrazka zastępczego
                        gameLogoImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/placeholder.png"));
                    }
                }
            }
        }
        private void UpdateUI()
        {
            GameNameTextBlock.Text = gameTitle ?? "Brak dostępnych informacji.";
            GameDeveloperTextBlock.Text = gameDeveloper ?? "Brak dostępnych informacji.";
            GameDescriptionTextBlock.Text = gameDescription ?? "Brak dostępnych informacji.";
            HardwareRequirementsTextBlock.Text = hardwareRequirements ?? "Brak dostępnych informacji.";
            OtherInformationsTextBlock.Text = otherInformations ?? "Brak dostępnych informacji.";
        }
        public GameBasePage()
        {
            ViewModel = App.GetService<GameBaseViewModel>();
            InitializeComponent();
            Status = LauncherStatus.pageLauched;
        }
        private async void IsUpdated()
        {
            if (File.Exists(gameExe))
            {
                SGSVersion.Version localVersion = new(File.ReadAllText(versionFile ?? ""));
                SGSVersion.Version onlineVersion = new(await httpClient.GetStringAsync(gameVersionLink));

                Status = LauncherStatus.ready;
                if (onlineVersion.IsDifferentThan(localVersion))
                {
                    CheckUpdateButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                }
            }
            else
            {
                Status = LauncherStatus.readyNoGame;
            }
        }
        private async void CheckForUpdates()
        {
            if (File.Exists(versionFile))
            {
                SGSVersion.Version localVersion = new(File.ReadAllText(versionFile));
                SGSVersion.Version onlineVersion = new(await httpClient.GetStringAsync(gameVersionLink));
                try
                {
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
                catch (Exception)
                {
                    Status = LauncherStatus.failed;
                }
            }
            else
            {
                InstallGameFiles(false, SGSVersion.Version.zero);
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
                    _onlineVersion = new SGSVersion.Version(await httpClient.GetStringAsync(gameVersionLink));
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
                        // Obsługa przypadku, gdy gameZip lub rootPath jest null
                        Status = LauncherStatus.failed;
                        return;
                    }
                }

                if (!string.IsNullOrEmpty(gameZip) && !string.IsNullOrEmpty(rootPath))
                {
                    ZipFile.ExtractToDirectory(gameZip, rootPath, true);
                    File.Delete(gameZip);
                }
                else
                {
                    // Obsługa przypadku, gdy gameZip lub rootPath jest null
                    Status = LauncherStatus.failed;
                    return;
                }

                if (!string.IsNullOrEmpty(versionFile))
                {
                    File.WriteAllText(versionFile, _onlineVersion.ToString());
                }
                else
                {
                    // Obsługa przypadku, gdy versionFile jest null
                    Status = LauncherStatus.failed;
                    return;
                }

                Status = LauncherStatus.ready;
                if (gamePayloadName != null)
                {
                    string localizedPayloadName = gamePayloadName.GetLocalized();
                    App.GetService<IAppNotificationService>().Show(string.Format(localizedPayloadName, AppContext.BaseDirectory));
                }
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
                File.Delete(versionFile ?? "");
                File.Delete(gameZip ?? "");

                Status = LauncherStatus.readyNoGame;
            }
            else
            {
                // Handle else case...
            }
        }
        private void UpdateClickButton(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            CheckForUpdates();
        }
        #endregion
    }
}