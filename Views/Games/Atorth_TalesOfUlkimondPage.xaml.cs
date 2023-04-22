using SGSClient.Controllers;
using SGSClient.ViewModels;
using Microsoft.UI.Xaml.Controls;
using System.Net;
using System.ComponentModel;
using System.IO.Compression;
using System.Diagnostics;
using Windows.ApplicationModel.Core;

namespace SGSClient.Views;
public sealed partial class Atorth_TalesOfUlkimondPage : Page
{
    private string rootPath; //określenie folderu klienta sgs
    private string gamepath; //okreslenie folderu z gra
    private string versionFile; //okreslenie pliku zawierającego wersję gry
    private string gameZip; //okreslenie zipu gry
    private string gameExe; //okreslenie pliku exe gry
    private LauncherStatus _status; //status plików gry

    WebClient webClient = new WebClient();

    //SGSVersion.Version sgsVersion = new SGSVersion.Version();
    internal LauncherStatus Status //co robic wedlug statusu gry
    {
        get => _status; //okresl status
        set
        {
            _status = value; //ustaw status
            switch (_status)
            {
                case LauncherStatus.ready: //jesli gra jest aktualna / gotowa do uruchomienia
                    //UpdateButton.Content = "Sprawdź aktualizację";
                    PlayButton.Content = "Graj";
                    PlayButton.IsEnabled = true;
                    DownloadProgressBorder.IsActive = false;
                    //DownloadProgressBar.Visibility = Visibility.Hidden;
                    UninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    break;
                case LauncherStatus.failed: //jesli gra nie zostala dobrze pobrana
                    //PlayButton.IsEnabled = false;
                    //UpdateButton.IsEnabled = true;
                    //UpdateButton.Content = "Spróbuj ponownie";
                    //if (File.Exists(gameExe))
                    //{
                    //    PlayButton.Content = "Graj";
                    //    UpdateButton.Content = "Sprawdź aktualizację";
                    //    PlayButton.IsEnabled = true;
                    //}
                    //else
                    //{
                    //    PlayButton.IsEnabled = false;
                    //}
                    //UpdateButton.IsEnabled = true;
                    UninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    DownloadProgressBorder.IsActive = false;
                    //DownloadProgressBar.Visibility = Visibility.Hidden;
                    break;
                case LauncherStatus.downloadingGame: //jesli gra jest pobierana z serwera
                    //PlayButton.Content = "Graj";
                    //UpdateButton.Content = "Pobieranie gry";
                    //PlayButton.IsEnabled = false;
                    //UpdateButton.IsEnabled = false;
                    UninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    DownloadProgressBorder.IsActive = true;
                    //DownloadProgressBar.Visibility = Visibility.Visible;
                    break;
                case LauncherStatus.downloadingUpdate: //jesli jest pobierany update gry
                    //PlayButton.Content = "Graj";
                    //UpdateButton.Content = "Aktualizowanie";
                    //PlayButton.IsEnabled = false;
                    //UpdateButton.IsEnabled = false;
                    //UninstallButton.Visibility = Visibility.Hidden;
                    DownloadProgressBorder.IsActive = true;
                    //DownloadProgressBar.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }
    }


    public Atorth_TalesOfUlkimondViewModel ViewModel
    {
        get;
    }
    public Atorth_TalesOfUlkimondPage()
    {
        ViewModel = App.GetService<Atorth_TalesOfUlkimondViewModel>();
        InitializeComponent();

        //rootPath = Directory.GetCurrentDirectory();
        var location = System.Reflection.Assembly.GetEntryAssembly().Location;
        rootPath = Path.GetDirectoryName(location);

        versionFile = Path.Combine(rootPath, "VersionATH.txt");
        gameZip = Path.Combine(rootPath, "AtorhTalesOfUlkimond.zip");
        gameExe = Path.Combine(rootPath, "AtorhTalesOfUlkimond", "game.exe");
        gamepath = Path.Combine(rootPath, "AtorhTalesOfUlkimond");

        DownloadProgressBorder.IsActive = false;
        UninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;

        isUpdated();

    }

    private void isUpdated()
    {
        if (File.Exists(gameExe))
        {
            PlayButton.Content = "Graj";
            //UpdateButton.Content = "Sprawdź aktualizację";
            //PlayButton.IsEnabled = true;
            UninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        }
        else
        {
            PlayButton.Content = "Zainstaluj";
            //PlayButton.IsEnabled = false;
            //UpdateButton.Content = "Pobierz grę";
            UninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }
    }
    private void checkForUpdates()
    {
        if (File.Exists(versionFile))
        {
            SGSVersion.Version localVersion = new SGSVersion.Version(File.ReadAllText(versionFile));
            SGSVersion.Version onlineVersion = new SGSVersion.Version(webClient.DownloadString("https://drive.google.com/uc?export=download&id=1vgPqhnKLny58SpFm8-bPZ8tzkeFmat7z")); //PLIK WERSJA GRY
            try
            {
                if (onlineVersion.IsDifferentThan(localVersion))
                {
                    InstallGameFiles(true, onlineVersion);
                }
                else
                {
                    Status = LauncherStatus.ready;
                }
            }
            catch (Exception ex)
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
                _onlineVersion = new SGSVersion.Version(webClient.DownloadString("https://drive.google.com/uc?export=download&id=1vgPqhnKLny58SpFm8-bPZ8tzkeFmat7z")); //PLIK WERSJA GRY
            }

            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCallback);
            webClient.DownloadFileAsync(new Uri("https://www.googleapis.com/drive/v3/files/1lYq27H-UBW4kRDr8tM5GSEEB2fmurZdc?alt=media&key=AIzaSyDh9_ofiUto8RWUDL2CwClUBbRNWHd3Yp4"), gameZip, _onlineVersion);

            webClient.DownloadProgressChanged += (s, e) =>
            {
                //DownloadProgressBar.Value = e.ProgressPercentage;
            };
            webClient.DownloadFileCompleted += (s, e) =>
            {
                DownloadProgressBorder.IsActive = false;
                //DownloadProgressBar.Visibility = Visibility.Hidden;
                // any other code to process the file

            };

        }
        catch (Exception ex)
        {
            Status = LauncherStatus.failed;
            //MessageBox.Show($"Błąd podczas instalowania plików gry: {ex}");
            //MessageBox.Show($"Błąd podczas instalowania plików gry. Spróbuj usunąć pliki gry klikając przycisk kosza znajdujący się poniżej.", "SGSClient", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }//instalacja plików gry
    private void DownloadGameCompletedCallback(object sender, AsyncCompletedEventArgs e) //zwracanie, że gra jest sciagnieta
    {
        try
        {
            string onlineVersion = ((SGSVersion.Version)e.UserState).ToString();
            ZipFile.ExtractToDirectory(gameZip, rootPath, true);
            File.Delete(gameZip);

            File.WriteAllText(versionFile, onlineVersion);

            //VersionText.Text = onlineVersion;
            Status = LauncherStatus.ready;
        }
        catch (Exception ex)
        {
            Status = LauncherStatus.failed;
            //MessageBox.Show($"Błąd podczas kończenia pobierania: {ex}. Spróbuj usunąć pliki gry klikając przycisk kosza znajdujący się poniżej.");
            //MessageBox.Show($"Błąd podczas pobierania plików gry. Spróbuj usunąć pliki gry klikając przycisk kosza znajdujący się poniżej, lub kliknij przycisk \"spróbuj ponownie\"", "SGSClient", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }


    private void playClickButton(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (File.Exists(gameExe))
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(gameExe);
                startInfo.WorkingDirectory = Path.Combine(rootPath, "AtorhTalesOfUlkimond");
                Process.Start(startInfo);
                //System.Windows.Application.Current.Shutdown();
                CoreApplication.Exit();
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Error: {ex}");
                CoreApplication.Exit();
                //System.Windows.Application.Current.Shutdown();
            }
        }
        else
        {
            try
            {
                SGSVersion.Version localVersion = new SGSVersion.Version(File.ReadAllText(versionFile));
                //VersionText.Text = localVersion.ToString();

                WebClient webClient = new WebClient();
                SGSVersion.Version onlineVersion = new SGSVersion.Version(webClient.DownloadString("https://drive.google.com/uc?export=download&id=18_AFfytSzP1EMt5cwGQGtcRvcDcu0RJO"));

                if (onlineVersion.IsDifferentThan(localVersion))
                {
                    //if (MessageBox.Show("Na serwerze została znaleziona nowsza wersja gry. Czy chcesz ją zaaktualizować?", "SGSClient", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    //{
                    ////jeśli nie to:
                    //}
                    //else
                    //{
                    //CheckForUpdates();//jeśli tak to
                    //}
                    checkForUpdates();
                }
                else if (File.Exists(gameExe) && Status == LauncherStatus.ready)
                {
                    //MessageBox.Show("Posiadasz aktualną wersję gry", "SGSClient", MessageBoxButton.OK, MessageBoxImage.Information);
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
            //MessageBox.Show($"Pomyślnie usunieto: Doddani", "SGSClient", MessageBoxButton.OK, MessageBoxImage.Information);

            //USTAWIENIE PRZYCISKÓW DO STANU POCZĄTKOWEGO
            PlayButton.Content = "Zainstaluj";
            UninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }
        else
        {
            //MessageBox.Show($"Nie zlokalizowano gry do usunięcia", "SGSClient", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

}
