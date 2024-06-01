using CommunityToolkit.Labs.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SGSClient.Core.Authorization;
using SGSClient.ViewModels;
using Windows.System;

namespace SGSClient.Views;

// TODO: Set the URL for your privacy policy by updating SettingsPage_PrivacyTermsLink.NavigateUri in Resources.resw.
public sealed partial class SettingsPage : Page
{
    public string Version
    {
        get
        {
            var version = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version;
            return string.Format("{0}.{1}.{2}.{3}", version?.Major, version?.Minor, version?.Build, version?.Revision);
        }
    }
    public SettingsViewModel ViewModel
    {
        get;
    }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();

        var session = SessionManager.LoadSession();
        if (session != null && session.IsLoggedIn)
            accountSettings.Visibility = Visibility.Visible;
        else
            accountSettings.Visibility = Visibility.Collapsed;
    }

    private async void bugRequestCard_Click(object sender, RoutedEventArgs e)
    {
        await Launcher.LaunchUriAsync(new Uri("https://github.com/szyrgamestudio/SGSClient/issues/new/choose"));
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        SessionManager.Logout();
        Frame.Navigate(typeof(LoginPage));
    }

}
