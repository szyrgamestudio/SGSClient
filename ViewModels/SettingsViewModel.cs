using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using SGSClient.Contracts.Services;
using SGSClient.Core.Authorization;
using SGSClient.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Input;
using Windows.ApplicationModel;

namespace SGSClient.ViewModels;
public class LanguageItem
{
    public LanguageItem(string tag, string name)
    {
        LanguageTag = tag;
        DisplayName = name;
    }

    public string LanguageTag { get; }
    public string DisplayName { get; }
}

public partial class SettingsViewModel : ObservableRecipient, INotifyPropertyChanged
{
    private readonly IThemeSelectorService _themeSelectorService;
    private INavigationService _navigationService;
    private readonly IAppUser _appUser;
    public bool IsLoggedIn;

    [ObservableProperty]
    private ElementTheme _elementTheme;

    [ObservableProperty]
    private string _versionDescription;

    public ICommand SwitchThemeCommand
    {
        get;
    }
    public ObservableCollection<LanguageItem> LanguageItems { get; } = new()
    {
        new("pl-PL", "Polski"),
        new("en-US", "English"),
    };

    private string selectedLanguage;
    public string SelectedLanguage
    {
        get => selectedLanguage;
        set => SetProperty(ref selectedLanguage, value);
    }

    public SettingsViewModel(IThemeSelectorService themeSelectorService, IAppUser appUser, INavigationService navigationService)
    {
        _themeSelectorService = themeSelectorService;
        _elementTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();
        _navigationService = navigationService;

        SwitchThemeCommand = new RelayCommand<ElementTheme>(
            async (param) =>
            {
                if (ElementTheme != param)
                {
                    ElementTheme = param;
                    await _themeSelectorService.SetThemeAsync(param);
                }
            });
        _appUser = appUser;
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
    public void LoadSession()
    {
        IsLoggedIn = _appUser.IsLoggedIn;
        OnPropertyChanged(nameof(IsLoggedIn));
    }
    public void SettingsCard_Click()
    {
        //_appUser.LoadSession();
        _navigationService.NavigateTo(typeof(SettingsUserViewModel).FullName!);
    }
}
