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
        private string gameIdentifier;
        private string? gameZipLink;
        private string? gameVersionLink;
        private string? gameTitle;
        private string? gameDeveloper;
        private string? gamePayloadName;
        private string? gameDescription;
        private string? hardwareRequirements;
        private string? otherInformations;

        private readonly HttpClient httpClient = new();
        ConfigurationManager configManager = new ConfigurationManager("D:\\DEVELOPMENT\\Repozytoria\\SGSClient\\Config\\appconfig.xml");
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

            versionFile = Path.Combine(rootPath, "versions.xml");
            gameZip = Path.Combine(rootPath, $"{gameIdentifier}.zip");
            gameExe = Path.Combine(rootPath, gameIdentifier ?? "", $"{configManager.GetGameExeName(gameIdentifier)}.exe");
            gamepath = Path.Combine(rootPath, gameIdentifier ?? "");
            UpdateUI();
            IsUpdated();
        }

        #region XML Handling
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
            // Ukryj OtherInformationsTextBlock, jeśli otherInformations jest puste
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
        }
        #endregion

        public GameBasePage()
        {
            ViewModel = App.GetService<GameBaseViewModel>();
            InitializeComponent();
            Status = LauncherStatus.pageLauched;
        }

        private async void IsUpdated()
        {
            try
            {
                XDocument versionXml;

                // Sprawdź, czy plik "versions.xml" istnieje
                if (File.Exists(Path.Combine(rootPath ?? "", "versions.xml")))
                {
                    // Jeśli istnieje, załaduj go
                    versionXml = XDocument.Load(Path.Combine(rootPath ?? "", "versions.xml"));
                }
                else
                {
                    // Jeśli nie istnieje, utwórz nowy dokument XML z korzeniem "Versions"
                    versionXml = new XDocument(new XElement("Versions"));
                }

                // Odczytaj lokalną wersję z pliku "versions.xml"
                XElement? gameVersionElement = versionXml.Root?.Element(gameIdentifier);

                SGSVersion.Version localVersion;

                if (gameVersionElement != null)
                {
                    localVersion = new SGSVersion.Version(gameVersionElement.Value);
                }
                else
                {
                    // Brak wersji dla konkretnej gry w pliku "versions.xml"
                    // Utwórz element dla gry
                    versionXml.Root?.Add(new XElement(gameIdentifier, "0.0.0.0"));
                    versionXml.Save(Path.Combine(rootPath ?? "", "versions.xml"));

                    localVersion = new SGSVersion.Version("0.0.0.0");

                    // Jeśli brak wersji, ustaw Status na readyNoGame
                    Status = LauncherStatus.readyNoGame;
                    return;
                }

                // Pobierz onlineVersion z pliku "appConfig.xml"
                XElement? gameElement = configManager.GetGameElement(gameIdentifier);
                string onlineVersionString = gameElement?.Element("GameVersion")?.Value ?? "0.0.0.0";
                SGSVersion.Version onlineVersion = new SGSVersion.Version(onlineVersionString);

                Status = LauncherStatus.ready;

                if (onlineVersion.IsDifferentThan(localVersion))
                {
                    CheckUpdateButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Status = LauncherStatus.failed;
            }
        }

        private async void CheckForUpdates()
        {
            try
            {
                XDocument versionXml;

                // Sprawdź, czy plik "versions.xml" istnieje
                if (File.Exists(Path.Combine(rootPath ?? "", "versions.xml")))
                {
                    // Jeśli istnieje, załaduj go
                    versionXml = XDocument.Load(Path.Combine(rootPath ?? "", "versions.xml"));
                }
                else
                {
                    // Jeśli nie istnieje, utwórz nowy dokument XML z korzeniem "Versions"
                    versionXml = new XDocument(new XElement("Versions"));
                }

                // Odczytaj lokalną wersję z pliku "versions.xml"
                XElement? gameVersionElement = versionXml.Root?.Element(gameIdentifier);

                SGSVersion.Version localVersion;

                if (gameVersionElement != null)
                {
                    localVersion = new SGSVersion.Version(gameVersionElement.Value);
                }
                else
                {
                    // Brak wersji dla konkretnej gry w pliku "versions.xml"
                    // Utwórz element dla gry
                    versionXml.Root?.Add(new XElement(gameIdentifier, "0.0.0.0"));
                    versionXml.Save(Path.Combine(rootPath ?? "", "versions.xml"));

                    localVersion = new SGSVersion.Version("0.0.0.0");
                }

                // Pobierz onlineVersion z pliku "appConfig.xml"
                XElement? gameElement = configManager.GetGameElement(gameIdentifier);
                string onlineVersionString = gameElement?.Element("GameVersion")?.Value ?? "0.0.0.0";
                SGSVersion.Version onlineVersion = new SGSVersion.Version(onlineVersionString);

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

                // Zapisz wersję do wspólnego pliku XML
                if (!string.IsNullOrEmpty(rootPath))
                {
                    XDocument versionXml;

                    // Sprawdź, czy istnieje plik XML z wersjami
                    string versionXmlPath = Path.Combine(rootPath, "versions.xml");
                    if (File.Exists(versionXmlPath))
                    {
                        versionXml = XDocument.Load(versionXmlPath);
                    }
                    else
                    {
                        // Jeśli plik nie istnieje, utwórz nowy
                        versionXml = new XDocument(new XElement("Versions"));
                    }

                    // Dodaj lub zaktualizuj informacje o wersji dla konkretnej gry
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
                    // Obsługa przypadku, gdy rootPath jest null
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

                // Usuń wpis dla konkretnej gry z pliku "versions.xml"
                XDocument versionXml;

                if (File.Exists(Path.Combine(rootPath ?? "", "versions.xml")))
                {
                    versionXml = XDocument.Load(Path.Combine(rootPath ?? "", "versions.xml"));

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
