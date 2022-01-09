using SGSClient;
using SGSClient.Core;
using SGSClient.MVVM.ViewModel;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace SGSClient.MVVM.View
{
    public partial class SciezkaBohateraView : UserControl
    {
        private string rootPath; //określenie folderu klienta sgs
        private string gamepath; //okreslenie folderu z gra
        private string versionFile; //okreslenie pliku zawierającego wersję gry
        private string gameZip; //okreslenie zipu gry
        private string gameExe; //okreslenie pliku exe gry

        private LauncherStatus _status; //status plików gry

        struct Version  //określenie co to zmienna version
        {
            internal static Version zero = new Version(0, 0, 0);

            private short major;
            private short minor;
            private short subMinor;

            internal Version(short _major, short _minor, short _subMinor)
            {
                major = _major;
                minor = _minor;
                subMinor = _subMinor;
            }
            internal Version(string _version)
            {
                string[] versionStrings = _version.Split('.');
                if (versionStrings.Length != 3)
                {
                    major = 0;
                    minor = 0;
                    subMinor = 0;
                    return;
                }

                major = short.Parse(versionStrings[0]);
                minor = short.Parse(versionStrings[1]);
                subMinor = short.Parse(versionStrings[2]);
            }

            internal bool IsDifferentThan(Version _otherVersion)
            {
                if (major != _otherVersion.major)
                {
                    return true;
                }
                else
                {
                    if (minor != _otherVersion.minor)
                    {
                        return true;
                    }
                    else
                    {
                        if (subMinor != _otherVersion.subMinor)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            public override string ToString()
            {
                return $"{major}.{minor}.{subMinor}";
            }
        }

        internal LauncherStatus Status //co robic wedlug statusu gry
        {
            get => _status; //okresl status
            set
            {
                _status = value; //ustaw status
                switch (_status)
                {
                    case LauncherStatus.ready: //jesli gra jest aktualna / gotowa do uruchomienia
                        UpdateButton.Content = "Sprawdź aktualizację";
                        PlayButton.Content = "Graj";
                        PlayButton.IsEnabled = true;
                        DownloadProgressBorder.Visibility = Visibility.Hidden;
                        DownloadProgressBar.Visibility = Visibility.Hidden;
                        UninstallButton.Visibility = Visibility.Visible;
                        break;
                    case LauncherStatus.failed: //jesli gra nie zostala dobrze pobrana
                        PlayButton.IsEnabled = false;
                        UpdateButton.IsEnabled = true;
                        UpdateButton.Content = "Spróbuj ponownie";
                        if (File.Exists(gameExe))
                        {
                            PlayButton.Content = "Graj";
                            UpdateButton.Content = "Sprawdź aktualizację";
                            PlayButton.IsEnabled = true;
                        }
                        else
                        {
                            PlayButton.IsEnabled = false;
                        }
                        UpdateButton.IsEnabled = true;
                        UninstallButton.Visibility = Visibility.Visible;
                        DownloadProgressBorder.Visibility = Visibility.Hidden;
                        DownloadProgressBar.Visibility = Visibility.Hidden;
                        break;
                    case LauncherStatus.downloadingGame: //jesli gra jest pobierana z serwera
                        PlayButton.Content = "Graj";
                        UpdateButton.Content = "Pobieranie gry";
                        PlayButton.IsEnabled = false;
                        UpdateButton.IsEnabled = false;
                        UninstallButton.Visibility = Visibility.Hidden;
                        DownloadProgressBorder.Visibility = Visibility.Visible;
                        DownloadProgressBar.Visibility = Visibility.Visible;
                        break;
                    case LauncherStatus.downloadingUpdate: //jesli jest pobierany update gry
                        PlayButton.Content = "Graj";
                        UpdateButton.Content = "Aktualizowanie";
                        PlayButton.IsEnabled = false;
                        UpdateButton.IsEnabled = false;
                        UninstallButton.Visibility = Visibility.Hidden;
                        DownloadProgressBorder.Visibility = Visibility.Visible;
                        DownloadProgressBar.Visibility = Visibility.Visible;
                        break;
                    default:
                        break;
                }
            }
        }

        public SciezkaBohateraView() //powoduje wyswietlenia okna w launcherze
        {
            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            versionFile = Path.Combine(rootPath, "VersionSCIEZKABOHATERA.txt");
            gameZip = Path.Combine(rootPath, "SciezkaBohatera.zip");
            gameExe = Path.Combine(rootPath, "SciezkaBohatera", "game.exe");
            gamepath = Path.Combine(rootPath, "SciezkaBohatera");


            isUpdated();
        }

        private void isUpdated() //po aktualizacji gry / odpalenie lauchera bez zainstalowanego atortha
        {
            if (File.Exists(gameExe))
            {
                PlayButton.Content = "Graj";
                UpdateButton.Content = "Sprawdź aktualizację";
                PlayButton.IsEnabled = true;
                UninstallButton.Visibility = Visibility.Visible;
            }
            else
            {
                PlayButton.IsEnabled = false;
                UpdateButton.Content = "Pobierz grę";
                UninstallButton.Visibility = Visibility.Hidden;
            }
        }

        private void CheckForUpdates()
        {

            if (File.Exists(versionFile))
            {
                Version localVersion = new Version(File.ReadAllText(versionFile));
                //VersionText.Text = localVersion.ToString();

                try
                {
                    //WebClient webClient = new WebClient();
                    WebClient webClient = new WebClient();
                    Version onlineVersion = new Version(webClient.DownloadString("https://www.googleapis.com/drive/v3/files/12pakYsS4YabHTASFoNXkBlxvVLHkCGja?alt=media&key=AIzaSyDh9_ofiUto8RWUDL2CwClUBbRNWHd3Yp4"));  //wersja gry

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
                    //MessageBox.Show($"Błąd podczas sprawdzania aktualizacji gry: {ex}");
                    MessageBox.Show($"Błąd podczas sprawdzania aktualizacji gry. Spróbuj usunąć pliki gry klikając przycisk kosza znajdujący się poniżej.", "SGSClient", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                InstallGameFiles(false, Version.zero);
            }
        }//sprawdzanie czy jest update

        private void InstallGameFiles(bool _isUpdate, Version _onlineVersion)
        {
            try
            {
                WebClient webClient = new WebClient();
                if (_isUpdate)
                {
                    Status = LauncherStatus.downloadingUpdate;
                }
                else
                {
                    Status = LauncherStatus.downloadingGame;
                    _onlineVersion = new Version(webClient.DownloadString("https://www.googleapis.com/drive/v3/files/12pakYsS4YabHTASFoNXkBlxvVLHkCGja?alt=media&key=AIzaSyDh9_ofiUto8RWUDL2CwClUBbRNWHd3Yp4")); //wersja gry
                }

                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCallback);
                webClient.DownloadFileAsync(new Uri("https://www.googleapis.com/drive/v3/files/1wZLPzd7GWIIxemdwq031AzCBxFtQVdnG?alt=media&key=AIzaSyDh9_ofiUto8RWUDL2CwClUBbRNWHd3Yp4"), gameZip, _onlineVersion); //zip gry

                webClient.DownloadProgressChanged += (s, e) =>
                {
                    DownloadProgressBar.Value = e.ProgressPercentage;
                };
                webClient.DownloadFileCompleted += (s, e) =>
                {
                    DownloadProgressBorder.Visibility = Visibility.Hidden;
                    DownloadProgressBar.Visibility = Visibility.Hidden;
                    // any other code to process the file

                };

            }
            catch (Exception ex)
            {
                Status = LauncherStatus.failed;
                //MessageBox.Show($"Błąd podczas instalowania plików gry: {ex}");
                MessageBox.Show($"Błąd podczas instalowania plików gry. Spróbuj usunąć pliki gry klikając przycisk kosza znajdujący się poniżej.", "SGSClient", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }//instalacja plików gry

        private void DownloadGameCompletedCallback(object sender, AsyncCompletedEventArgs e) //zwracanie, że gra jest sciagnieta
        {
            try
            {
                string onlineVersion = ((Version)e.UserState).ToString();
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
                MessageBox.Show($"Błąd podczas pobierania plików gry. Spróbuj usunąć pliki gry klikając przycisk kosza znajdujący się poniżej, lub kliknij przycisk \"spróbuj ponownie\"", "SGSClient", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {

            if (File.Exists(gameExe))
            {
                PlayButton.IsEnabled = true;
                DownloadProgressBorder.Visibility = Visibility.Hidden;
                DownloadProgressBar.Visibility = Visibility.Hidden;
            }
            //CheckForUpdates();
        }


        //PRZYCISKI
        private void updateClickButton(object sender, RoutedEventArgs e)
        {
            try
            {
                Version localVersion = new Version(File.ReadAllText(versionFile));
                //VersionText.Text = localVersion.ToString();

                WebClient webClient = new WebClient();
                Version onlineVersion = new Version(webClient.DownloadString("https://www.googleapis.com/drive/v3/files/12pakYsS4YabHTASFoNXkBlxvVLHkCGja?alt=media&key=AIzaSyDh9_ofiUto8RWUDL2CwClUBbRNWHd3Yp4")); //WERSJA GRY

                if (onlineVersion.IsDifferentThan(localVersion))
                {
                    if (MessageBox.Show("Na serwerze została znaleziona nowsza wersja gry. Czy chcesz ją zaaktualizować?", "SGSClient", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                    {
                        //jeśli nie to:
                    }
                    else
                    {
                        CheckForUpdates();//jeśli tak to
                    }
                    // CheckForUpdates();
                }
                else if (File.Exists(gameExe) && Status == LauncherStatus.ready)
                {
                    MessageBox.Show("Posiadasz aktualną wersję gry", "SGSClient", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (Status == LauncherStatus.failed)
                {
                    CheckForUpdates();
                }
                else
                {
                    CheckForUpdates();
                }
            }
            catch (Exception ex)
            {
                CheckForUpdates();
            }


        }

        private void playClickButton(object sender, RoutedEventArgs e)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(gameExe);
                startInfo.WorkingDirectory = Path.Combine(rootPath, "SciezkaBohatera");
                Process.Start(startInfo);
                System.Windows.Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex}");
                System.Windows.Application.Current.Shutdown();
            }


        }

        private void uninstallButton(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(gamepath))
            {
                if (MessageBox.Show("Odinstalowanie gry będzie skutkowało usunięciem wszystkich plików związanych z grą Ścieżka Bohatera. Czy aby na pewno kontynuować? (Upewnij się, że wykonałeś kopię swoich zapisów gry, jeśli natomiast kliknąłeś ten przycisk z powodu niepoprawnego ściągniecia się plików, możesz ten komunikat zignorować.)", "UWAGA!", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    //jeśli nie to:
                }
                else
                {
                    //jeśli tak to:
                    Directory.Delete(gamepath, true);
                    File.Delete(versionFile);
                    File.Delete(gameZip);
                    MessageBox.Show($"Pomyślnie usunieto: Ścieżka Bohatera", "SGSClient", MessageBoxButton.OK, MessageBoxImage.Information);

                    //USTAWIENIE PRZYCISKÓW DO STANU POCZĄTKOWEGO
                    PlayButton.IsEnabled = false;
                    UpdateButton.IsEnabled = true;
                    PlayButton.Content = "Graj";
                    UpdateButton.Content = "Pobierz grę";
                    UninstallButton.Visibility = Visibility.Hidden;

                }

            }
            else
            {
                MessageBox.Show($"Nie zlokalizowano gry do usunięcia", "SGSClient", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

    }

}
