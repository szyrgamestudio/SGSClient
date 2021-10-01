using SGS_Client;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;

namespace GameLauncher
{
    enum LauncherStatus
    {
        ready,
        failed,
        downloadingGame,
        downloadingUpdate
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class Atorth : Window
    {
        private string rootPath;
        private string versionFile;
        private string gameZip;
        private string gameExe;

        private LauncherStatus _status;

        internal LauncherStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                switch (_status)
                {
                    case LauncherStatus.ready:
                        PlayButton.Content = "Gra Aktualna";
                        Graj.IsEnabled = true;
                        Menu.IsEnabled = true;
                        PlayButton.IsEnabled = true;
                        Exit.IsEnabled = true;
                        pBar1.Visibility = Visibility.Hidden;
                        break;
                    case LauncherStatus.failed:
                        PlayButton.Content = "Aktualizacja nie udana";
                        if (File.Exists(gameExe))
                        {
                            Graj.IsEnabled = true;
                        }
                        else
                        {
                            Graj.IsEnabled = false;
                        }
                        Menu.IsEnabled = true;
                        PlayButton.IsEnabled = true;
                        Exit.IsEnabled = true;
                        pBar1.Visibility = Visibility.Hidden;
                        break;
                    case LauncherStatus.downloadingGame:
                        PlayButton.Content = "Pobieranie gry";
                        Graj.IsEnabled = false;
                        Menu.IsEnabled = false;
                        PlayButton.IsEnabled = false;
                        Exit.IsEnabled = false;
                        pBar1.Visibility = Visibility.Visible;
                        break;
                    case LauncherStatus.downloadingUpdate:
                        PlayButton.Content = "Aktualizowanie";
                        Graj.IsEnabled = false;
                        Menu.IsEnabled = false;
                        PlayButton.IsEnabled = false;
                        Exit.IsEnabled = false;
                        pBar1.Visibility = Visibility.Visible;
                        break;
                    default:
                        break;
                }
            }
        }

        public Atorth()
        {
            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            versionFile = Path.Combine(rootPath, "VersionATH.txt");
            gameZip = Path.Combine(rootPath, "AtorhTalesOfUlkimond.zip");
            gameExe = Path.Combine(rootPath, "AtorhTalesOfUlkimond", "game.exe");


            poUpdate();

        }

        private void poUpdate()
        {
            if (File.Exists(gameExe))
            {
                Graj.IsEnabled = true;
            }
            else
            {
                Graj.IsEnabled = false;
            }
        }

        private void CheckForUpdates()
        {
            
            if (File.Exists(versionFile))
            {
                Version localVersion = new Version(File.ReadAllText(versionFile));
                VersionText.Text = localVersion.ToString();

                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersion = new Version(webClient.DownloadString("https://drive.google.com/uc?export=download&id=18_AFfytSzP1EMt5cwGQGtcRvcDcu0RJO"));

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
                    MessageBox.Show($"Błąd podczas sprawdzania aktualizacji gry: {ex}");
                }
            }
            else
            {
                InstallGameFiles(false, Version.zero);
            }
        }

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
                    _onlineVersion = new Version(webClient.DownloadString("https://drive.google.com/uc?export=download&id=18_AFfytSzP1EMt5cwGQGtcRvcDcu0RJO"));
                }

                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCallback);
                webClient.DownloadFileAsync(new Uri("https://www.googleapis.com/drive/v3/files/1lYq27H-UBW4kRDr8tM5GSEEB2fmurZdc?alt=media&key=AIzaSyDh9_ofiUto8RWUDL2CwClUBbRNWHd3Yp4"), gameZip, _onlineVersion);

                webClient.DownloadProgressChanged += (s, e) =>
                {
                    pBar1.Value = e.ProgressPercentage;
                };
                webClient.DownloadFileCompleted += (s, e) =>
                {
                    pBar1.Visibility = Visibility.Hidden;
                    // any other code to process the file

                };
                
            }
            catch (Exception ex)
            {
                Status = LauncherStatus.failed;
                MessageBox.Show($"Błąd podczas instalowania plików gry: {ex}"); 
            }
        }

        private void DownloadGameCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                string onlineVersion = ((Version)e.UserState).ToString();
                ZipFile.ExtractToDirectory(gameZip, rootPath, true);
                File.Delete(gameZip);

                File.WriteAllText(versionFile, onlineVersion);

                VersionText.Text = onlineVersion;
                Status = LauncherStatus.ready;
            }
            catch (Exception ex)
            {
                Status = LauncherStatus.failed;
                MessageBox.Show($"Błąd podczas kończenia pobierania: {ex}");
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {

            if (File.Exists(gameExe))
            {
                Graj.IsEnabled = true;
                pBar1.Visibility = Visibility.Hidden;
            }
            //CheckForUpdates();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Version localVersion = new Version(File.ReadAllText(versionFile));
                VersionText.Text = localVersion.ToString();

                WebClient webClient = new WebClient();
                Version onlineVersion = new Version(webClient.DownloadString("https://drive.google.com/uc?export=download&id=18_AFfytSzP1EMt5cwGQGtcRvcDcu0RJO"));

                if (onlineVersion.IsDifferentThan(localVersion))
                {
                    CheckForUpdates();
                }
                else if (File.Exists(gameExe) && Status == LauncherStatus.ready)
                {
                    MessageBox.Show("Posiadasz aktualną wersję gry");
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Hide();
            Main Mn = new Main();
            Mn.ShowDialog();
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(gameExe);
            startInfo.WorkingDirectory = Path.Combine(rootPath, "AtorhTalesOfUlkimond");
            Process.Start(startInfo);

            System.Windows.Application.Current.Shutdown();
        }

        private void Button_Click_Wyjscie(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }

    struct Version
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
}
