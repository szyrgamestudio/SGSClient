using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.ApplicationModel.Resources;
using SGSClient.ViewModels;
using Windows.Globalization;
using Windows.Storage;
using Windows.System;

namespace SGSClient.Views;

public sealed partial class SettingsPage : Page
{
    #region Fields
    private readonly ResourceManager resourceManager = new();
    private readonly ResourceLoader resourceLoader = new();
    #endregion

    #region Properties
    public string Version
    {
        get
        {
            var version = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version;
            return string.Format("{0}.{1}.{2}.{3}", version?.Major, version?.Minor, version?.Build, version?.Revision);
        }
    }
    public SettingsViewModel ViewModel { get; }
    #endregion

    #region Ctor
    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();

        ViewModel.LoadSession();
        accountSettingsSP.Visibility = ViewModel.IsLoggedIn ? Visibility.Visible : Visibility.Collapsed;
        accountSettingsCard.Visibility = ViewModel.IsLoggedIn ? Visibility.Visible : Visibility.Collapsed;
    }
    #endregion

    #region Event Handlers
    private async void bugRequestCard_Click(object sender, RoutedEventArgs e)
    {
        await Launcher.LaunchUriAsync(new Uri("https://github.com/szyrgamestudio/SGSClient/issues/new/choose"));
    }
    private void SettingsCard_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SettingsCard_Click();
    }
    private void LanguageSelector_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not ComboBox combo)
            return;

        string savedLang = ApplicationData.Current.LocalSettings.Values["AppLanguage"] as string
                           ?? ApplicationLanguages.PrimaryLanguageOverride
                           ?? "pl-PL";

        ViewModel.SelectedLanguage = savedLang;
        combo.SelectedValue = savedLang;
    }

    private void LanguageSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox combo || combo.SelectedValue is not string langTag)
            return;

        ApplicationData.Current.LocalSettings.Values["AppLanguage"] = langTag;
        ApplicationLanguages.PrimaryLanguageOverride = langTag;

        Helpers.LocalizedText.Instance.UpdateLanguage(langTag);
    }
    #endregion
}
