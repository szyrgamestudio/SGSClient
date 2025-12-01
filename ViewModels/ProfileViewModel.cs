using CommunityToolkit.Mvvm.ComponentModel;
using SGSClient.Core.Authorization;
using SGSClient.Core.Models;
using SGSClient.Core.Utilities.AppInfoUtility.Interfaces;
using System.Collections.ObjectModel;

namespace SGSClient.ViewModels;

public class ProfileViewModel : ObservableObject
{
    private readonly IAppUser _appUser;
    private readonly IAppInfo _appInfo;
    public ProfileViewModel(IAppUser appUser, IAppInfo appInfo)
    {
        _appUser = appUser;
        _appInfo = appInfo;
    }
    public ObservableCollection<UserGameInfo> RecentlyPlayed { get; private set; } = new();

    public void LoadRecentlyPlayed()
    {
        RecentlyPlayed.Clear();
        var data = _appUser.GetUserGameInfo();

        foreach (var g in data.Take(10))
            RecentlyPlayed.Add(g);
    }
}
