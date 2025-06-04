using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Navigation;
using SGSClient.Contracts.Services;
using SGSClient.Core.Authorization;
using SGSClient.Core.Database;
using SGSClient.Core.Extensions;
using System.Windows.Input;

namespace SGSClient.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    private readonly IAppUser _appUser;
    [ObservableProperty]
    private bool isBackEnabled;

    [ObservableProperty]
    private object? selected;

    [ObservableProperty]
    private string userDisplayName;

    [ObservableProperty]
    private string? userEmail;

    [ObservableProperty]
    private bool isUserLoggedIn;

    [ObservableProperty]
    private string userMenuText; // Wartość używana w XAML

    public ICommand LoginCommand { get; }
    public ICommand LogoutCommand { get; }
    public ICommand MyGamesCommand { get; }
    public ICommand AddGameCommand { get; }

    public INavigationService NavigationService { get; }
    public INavigationViewService NavigationViewService { get; }

    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService, IAppUser appUser)
    {
        NavigationService = navigationService;
        NavigationService.Navigated += OnNavigated;
        NavigationViewService = navigationViewService;

        _appUser = appUser; // Dependency Injection zamiast AppUser.Current
        LoginCommand = new AsyncRelayCommand(ExecuteLoginAsync);
        LogoutCommand = new AsyncRelayCommand(ExecuteLogoutAsync);
        MyGamesCommand = new RelayCommand(NavigateToMyGames);
        AddGameCommand = new RelayCommand(NavigateToUpload);

        _appUser.TrySilentLoginAsync();
        UpdateUserData();
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = NavigationService.CanGoBack;

        if (e.SourcePageType == typeof(SGSClient.Views.SettingsPage))
        {
            Selected = NavigationViewService.SettingsItem;
            return;
        }

        var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
        if (selectedItem != null)
        {
            Selected = selectedItem;
        }

        #region Database Update

        if (!string.IsNullOrWhiteSpace(_appUser.DisplayName) && !string.IsNullOrWhiteSpace(_appUser.UserId))
        {
            db.con.exec(@"
insert Users(DisplayName, UserId)
select
  @p0
, @p1
from (select 1 x) x
left join Users u on u.UserId = @p1
where u.UserId is null

update u set
  u.DisplayName = @p0
from Users u
where u.UserId = @p1 and u.DisplayName != @p0
", _appUser.DisplayName.ToString(), _appUser.UserId.ToString());
        }
        #endregion

    }

    private void UpdateUserData()
    {
        IsUserLoggedIn = _appUser.IsLoggedIn;
        UserDisplayName = IsUserLoggedIn ? _appUser.DisplayName : "Zaloguj się";
        UserEmail = IsUserLoggedIn ? _appUser.Email : null;
        UserMenuText = IsUserLoggedIn ? _appUser.DisplayName : "Zaloguj się";
    }

    private async Task ExecuteLoginAsync()
    {
        if (await _appUser.LoginAsync(IntPtr.Zero, false))
        {
            await _appUser.LoadUserDataAsync();
            UpdateUserData();
        }
    }

    private async Task ExecuteLogoutAsync()
    {
        await _appUser.LogoutAsync(IntPtr.Zero);
        UpdateUserData();
    }

    public string GetUserDisplayNameAsync()
    {
        if (_appUser.IsLoggedIn)
        {
            return _appUser.UserId;
        }
        else
        {
            return null;
        }
    }

    public void NavigateToMyGames()
    {
        NavigationService.NavigateTo(typeof(MyGamesViewModel).FullName!);
    }

    public void NavigateToUpload()
    {
        NavigationService.NavigateTo(typeof(UploadGameViewModel).FullName!);
    }
}
