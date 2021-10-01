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
    enum LauncherStatus3
    {
        ready,
        failed,
        downloadingGame,
        downloadingUpdate
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class StaffOfHell : Window
    {
        private string rootPath;
        private string versionFile;
        private string gameZip;
        private string gameExe;

        private LauncherStatus3 _status;
        internal LauncherStatus3 Status
        {
            get => _status;
            set
            {
                _status = value;
                switch (_status)
                {
                    case LauncherStatus3.ready:
                        PlayButton.Content = "Graj";
                        break;
                    case LauncherStatus3.failed:
                        PlayButton.Content = "Aktualizacja nie udana";
                        break;
                    case LauncherStatus3.downloadingGame:
                        PlayButton.Content = "Pobieranie gry";
                        break;
                    case LauncherStatus3.downloadingUpdate:
                        PlayButton.Content = "Aktualizowanie";
                        break;
                    default:
                        break;
                }
            }
        }

        public StaffOfHell()
        {
            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            versionFile = Path.Combine(rootPath, "VersionSOH.txt");
            gameZip = Path.Combine(rootPath, "StaffOfHell.zip");
            gameExe = Path.Combine(rootPath, "StaffOfHell", "Game.exe");
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
                    Version onlineVersion = new Version(webClient.DownloadString("https://drive.google.com/uc?export=download&id=1fJDU2sPSHew_zV598xqXZltnMjaouWxu"));

                    if (onlineVersion.IsDifferentThan(localVersion))
                    {
                        InstallGameFiles(true, onlineVersion);
                    }
                    else
                    {
                        Status = LauncherStatus3.ready;
                    }
                }
                catch (Exception ex)
                {
                    Status = LauncherStatus3.failed;
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
                    Status = LauncherStatus3.downloadingUpdate;
                }
                else
                {
                    Status = LauncherStatus3.downloadingGame;
                    _onlineVersion = new Version(webClient.DownloadString("https://drive.google.com/uc?export=download&id=1fJDU2sPSHew_zV598xqXZltnMjaouWxu"));
                }

                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCallback);
                webClient.DownloadFileAsync(new Uri("https://www.googleapis.com/drive/v3/files/1QA8zOVKz4Mlp4lb9GVSDVk1GpyGpYGNS?alt=media&key=AIzaSyDh9_ofiUto8RWUDL2CwClUBbRNWHd3Yp4"), gameZip, _onlineVersion);
            }
            catch (Exception ex)
            {
                Status = LauncherStatus3.failed;
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
                Status = LauncherStatus3.ready;
            }
            catch (Exception ex)
            {
                Status = LauncherStatus3.failed;
                MessageBox.Show($"Błąd podczas kończenia pobierania: {ex}");
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            CheckForUpdates();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(gameExe) && Status == LauncherStatus3.ready)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(gameExe);
                startInfo.WorkingDirectory = Path.Combine(rootPath, "StaffOfHell");
                Process.Start(startInfo);

                Close();
            }
            else if (Status == LauncherStatus3.failed)
            {
                CheckForUpdates();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            Atorth Ath = new Atorth();
            Ath.ShowDialog();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Hide();
            Main Mn = new Main();
            Mn.ShowDialog();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.Hide();
            Doddani Dod = new Doddani();
            Dod.ShowDialog();
        }
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            this.Hide();
            StaffOfHell SOH = new StaffOfHell();
            SOH.ShowDialog();
        }
    }

    struct Version3
    {
        internal static Version3 zero = new Version3(0, 0, 0);

        private short major;
        private short minor;
        private short subMinor;

        internal Version3(short _major, short _minor, short _subMinor)
        {
            major = _major;
            minor = _minor;
            subMinor = _subMinor;
        }
        internal Version3(string _version)
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

        internal bool IsDifferentThan(Version3 _otherVersion)
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
