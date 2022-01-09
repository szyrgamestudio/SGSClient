using System.Windows;
using System.Net;
using System.Diagnostics;
using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Windows.Controls;

namespace SGSClient
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
        private string rootPath; //określenie folderu klienta sgs
        private string versionFile; //okreslenie pliku zawierającego wersję gry
        public MainWindow()
        {
            InitializeComponent();

            WebClient webClient = new WebClient();
            rootPath = Directory.GetCurrentDirectory();
            versionFile = Path.Combine(rootPath, "versionSGSCLIENT.txt");
            Version localVersion = new Version(File.ReadAllText(versionFile));
            Version onlineVersion = new Version(webClient.DownloadString("https://www.googleapis.com/drive/v3/files/1r7IhY3CqprclQz7r8q7k7yywxquF87wA?alt=media&key=AIzaSyDh9_ofiUto8RWUDL2CwClUBbRNWHd3Yp4"));

            try
            {
                if (onlineVersion.IsDifferentThan(localVersion))
                {
                    if (MessageBox.Show("Na serwerze znaleziono nowszą wersję SGSLaunchera. Czy chcesz ją pobrać?", "SGSClient", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        using (var client = new WebClient())
                        {
                            Process.Start("SGSClientUpdater.exe");
                            this.Close();
                        }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
