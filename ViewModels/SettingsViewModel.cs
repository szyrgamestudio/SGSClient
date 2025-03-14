﻿using System.ComponentModel;
using System.Reflection;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using SGSClient.Contracts.Services;
using SGSClient.Core.Authorization;
using SGSClient.Helpers;

using Windows.ApplicationModel;

namespace SGSClient.ViewModels;

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
