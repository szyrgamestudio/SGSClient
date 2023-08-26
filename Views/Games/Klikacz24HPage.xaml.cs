using SGSClient.Controllers;
using SGSClient.ViewModels;
using Microsoft.UI.Xaml.Controls;
using System.Net;
using System.ComponentModel;
using System.IO.Compression;
using System.Diagnostics;
using Windows.ApplicationModel.Core;
using static System.Net.WebRequestMethods;
using File = System.IO.File;
using Windows.Storage;
using SGSClient.Contracts.Services;
using SGSClient.Helpers;
using System.Windows;

namespace SGSClient.Views;

public sealed partial class Klikacz24HPage : Page
{
    private readonly string rootPath; //określenie folderu klienta sgs
    private readonly string gamepath; //okreslenie folderu z gra
    private readonly string versionFile; //okreslenie pliku zawierającego wersję gry
    private readonly string gameZip; //okreslenie zipu gry
    private readonly string gameExe; //okreslenie pliku exe gry
    private LauncherStatus _status; //status plików gry

    private readonly string gameZipLink = "https://onedrive.live.com/download?resid=6B420D3CABAB13DF%211265133&authkey=!AI9RR6Ly3P6NwRY";
    private readonly string gameVersionLink = "https://onedrive.live.com/download?resid=6B420D3CABAB13DF%211265134&authkey=!ABGVvWkxhwhuxJY";

    WebClient webClient = new WebClient();
    internal LauncherStatus Status //co robic wedlug statusu gry
    {
        get => _status; //okresl status
        set
        {
            _status = value; //ustaw status
            switch (_status)
            {
                case LauncherStatus.ready: //jesli gra jest aktualna / gotowa do uruchomienia
                    PlayButton.Content = "Graj";
                    PlayButton.IsEnabled = true;
                    DownloadProgressBorder.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    UninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    File.Delete(gameZip);
                    break;
                case LauncherStatus.failed: //jesli gra nie zostala dobrze pobrana
                    UninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    DownloadProgressBorder.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    break;
                case LauncherStatus.downloadingGame: //jesli gra jest pobierana z serwera
                    UninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    DownloadProgressBorder.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    break;
                case LauncherStatus.downloadingUpdate: //jesli jest pobierany update gry
                    UninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    DownloadProgressBorder.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    break;
                default:
                    break;
            }
        }
    }
    public Klikacz24HViewModel ViewModel
    {
        get;
    }
    public Klikacz24HPage()
    {
        ViewModel = App.GetService<Klikacz24HViewModel>();
        InitializeComponent();

        string location = Path.Combine(ApplicationData.Current.LocalFolder.Path, "LocalState");
        rootPath = Path.GetDirectoryName(location);

        versionFile = Path.Combine(rootPath, "versionKlikacz24H.txt");
        gameZip = Path.Combine(rootPath, "Klikacz24H.zip");
        gameExe = Path.Combine(rootPath, "Klikacz24H", "gra24h.exe");
        gamepath = Path.Combine(rootPath, "Klikacz24H");

        DownloadProgressBorder.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        UninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;

        isUpdated();
    }
    private void isUpdated()
    {
        if (File.Exists(gameExe))
        {
            SGSVersion.Version localVersion = new SGSVersion.Version(File.ReadAllText(versionFile));
            SGSVersion.Version onlineVersion = new SGSVersion.Version(webClient.DownloadString(gameVersionLink)); //PLIK WERSJA GRY

            PlayButton.Content = "Graj";
            UninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            if (!onlineVersion.IsDifferentThan(localVersion))
            {
                CheckUpdateButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            }
            Status = LauncherStatus.ready;
        }
        else
        {
            PlayButton.Content = "Zainstaluj";
            UninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            CheckUpdateButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }
    }
    private async void checkForUpdates()
    {
        if (File.Exists(versionFile))
        {
            SGSVersion.Version localVersion = new SGSVersion.Version(File.ReadAllText(versionFile));
            SGSVersion.Version onlineVersion = new SGSVersion.Version(webClient.DownloadString(gameVersionLink)); //PLIK WERSJA GRY
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
    private void InstallGameFiles(bool _isUpdate, SGSVersion.Version _onlineVersion)
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
                _onlineVersion = new SGSVersion.Version(webClient.DownloadString(gameVersionLink)); //PLIK WERSJA GRY
            }

            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCallback);
            webClient.DownloadFileAsync(new Uri(gameZipLink), gameZip, _onlineVersion);

            webClient.DownloadProgressChanged += (s, e) =>
            {
                //DownloadProgressBar.Value = e.ProgressPercentage;
            };
            webClient.DownloadFileCompleted += (s, e) =>
            {
                DownloadProgressBorder.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                CheckUpdateButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;

                //DownloadProgressBar.Visibility = Visibility.Hidden;
                // any other code to process the file
            };
        }
        catch (Exception)
        {
            Status = LauncherStatus.failed;
        }
    }
    //instalacja plików gry
    private void DownloadGameCompletedCallback(object sender, AsyncCompletedEventArgs e) //zwracanie, że gra jest sciagnieta
    {
        try
        {
            string onlineVersion = ((SGSVersion.Version)e.UserState).ToString();
            ZipFile.ExtractToDirectory(gameZip, rootPath, true);
            //File.Delete(gameZip);

            File.WriteAllText(versionFile, onlineVersion);

            Status = LauncherStatus.ready;
            App.GetService<IAppNotificationService>().Show(string.Format("Klikacz24HNotificationPayload".GetLocalized(), AppContext.BaseDirectory));

        }
        catch (Exception ex)
        {
            Status = LauncherStatus.failed;
        }
    }
    private void playClickButton(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (File.Exists(gameExe))
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(gameExe);
                startInfo.WorkingDirectory = Path.Combine(rootPath, "Klikacz24H");
                Process.Start(startInfo);
                CoreApplication.Exit();
            }
            catch (Exception ex)
            {
                CoreApplication.Exit();
            }
        }
        else
        {
            try
            {
                SGSVersion.Version localVersion = new SGSVersion.Version(File.ReadAllText(versionFile));

                WebClient webClient = new WebClient();
                SGSVersion.Version onlineVersion = new SGSVersion.Version(webClient.DownloadString(gameVersionLink));

                if (onlineVersion.IsDifferentThan(localVersion))
                {
                    checkForUpdates();
                }
                else if (File.Exists(gameExe) && Status == LauncherStatus.ready)
                {
                }
                else if (Status == LauncherStatus.failed)
                {
                    checkForUpdates();
                }
                else
                {
                    checkForUpdates();
                }
            }
            catch (Exception ex)
            {
                checkForUpdates();
            }

        }
    }
    private void uninstallClickButton(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (Directory.Exists(gamepath))
        {
            uninstallFlyout.Hide();
            Directory.Delete(gamepath, true);
            File.Delete(versionFile);
            File.Delete(gameZip);

            //USTAWIENIE PRZYCISKÓW DO STANU POCZĄTKOWEGO
            PlayButton.Content = "Zainstaluj";
            UninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            CheckUpdateButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;

        }
        else
        {
        }
    }
    private void updateClickButton(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        checkForUpdates();
    }

}
