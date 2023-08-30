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

namespace SGSClient.Views
{
    public sealed partial class BlackWhiteJumpPage : Page
    {
        private readonly string rootPath;
        private readonly string gamepath;
        private readonly string versionFile;
        private readonly string gameZip;
        private readonly string gameExe;
        private LauncherStatus _status;

        private readonly string gameZipLink = "https://dl.dropboxusercontent.com/scl/fi/n95vbm3jdvzvrjd0qwv42/BlackWhiteJump.zip?rlkey=nkyob4e67xena53rfdb4v4prb&dl=0";
        private readonly string gameVersionLink = "https://dl.dropboxusercontent.com/scl/fi/5zacox2h5shimjhk5a8wy/versionBlackWhiteJump.txt?rlkey=c8gs3lxpsm6a79hi997igu4vl&dl=0";

        private readonly HttpClient httpClient = new();

        internal LauncherStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                switch (_status)
                {
                    case LauncherStatus.pageLauched:
                        PlayButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                        CheckUpdateButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                        UninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                        DownloadProgressBorder.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                        break;
                    case LauncherStatus.readyNoGame:
                        PlayButton.Content = "Zainstaluj";
                        PlayButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                        DownloadProgressBorder.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                        UninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                        CheckUpdateButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                        break;
                    case LauncherStatus.ready:
                        PlayButton.Content = "Graj";
                        PlayButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                        DownloadProgressBorder.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                        UninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                        DownloadProgressBorder.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                        File.Delete(gameZip); //delete file zip (free memory is important)
                        break;
                    case LauncherStatus.failed:
                        UninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                        DownloadProgressBorder.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                        break;
                    case LauncherStatus.downloadingGame:
                        PlayButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                        UninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                        DownloadProgressBorder.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                        break;
                    case LauncherStatus.downloadingUpdate:
                        PlayButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                        UninstallButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                        DownloadProgressBorder.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                        break;
                    default:
                        break;
                }
            }
        }

        public BlackWhiteJumpViewModel ViewModel { get; }
        public BlackWhiteJumpPage()
        {
            ViewModel = App.GetService<BlackWhiteJumpViewModel>();
            InitializeComponent();

            string location = Path.Combine(ApplicationData.Current.LocalFolder.Path, "LocalState");
            rootPath = Path.GetDirectoryName(location) ?? string.Empty;

            versionFile = Path.Combine(rootPath, "versionBlackWhiteJump.txt");
            gameZip = Path.Combine(rootPath, "BlackWhiteJump.zip");
            gameExe = Path.Combine(rootPath, "BlackWhiteJump", "BlackWhiteJump.exe");
            gamepath = Path.Combine(rootPath, "BlackWhiteJump");

            Status = LauncherStatus.pageLauched;
            IsUpdated();
        }
        private async void IsUpdated()
        {
            if (File.Exists(gameExe))
            {
                SGSVersion.Version localVersion = new(File.ReadAllText(versionFile));
                SGSVersion.Version onlineVersion = new(await httpClient.GetStringAsync(gameVersionLink));

                if (!onlineVersion.IsDifferentThan(localVersion))
                {
                    CheckUpdateButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                }
                Status = LauncherStatus.ready;
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

                HttpResponseMessage response = await httpClient.GetAsync(new Uri(gameZipLink));
                response.EnsureSuccessStatusCode();

                using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                {
                    using FileStream fileStream = new(gameZip, FileMode.Create, FileAccess.Write, FileShare.None);
                    await contentStream.CopyToAsync(fileStream);
                }

                ZipFile.ExtractToDirectory(gameZip, rootPath, true);
                File.Delete(gameZip);

                File.WriteAllText(versionFile, _onlineVersion.ToString());

                Status = LauncherStatus.ready;
                App.GetService<IAppNotificationService>().Show(string.Format("BlackWhiteJumpNotificationPayload".GetLocalized(), AppContext.BaseDirectory));
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
                        WorkingDirectory = Path.Combine(rootPath, "BlackWhiteJump")
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
                File.Delete(versionFile);
                File.Delete(gameZip);

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