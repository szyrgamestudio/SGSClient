using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Controls;

using SGSClient.Contracts.Services;
using SGSClient.ViewModels;
using SGSClient.Views;

namespace SGSClient.Services;

public class PageService : IPageService
{
    private readonly Dictionary<string, Type> _pages = new();

    public PageService()
    {
        Configure<HomeViewModel, HomePage>();
        Configure<GamesViewModel, GamesPage>();
        Configure<SettingsViewModel, SettingsPage>();
        Configure<Atorth_TalesOfUlkimondViewModel, Atorth_TalesOfUlkimondPage>();
        Configure<DoddaniViewModel, DoddaniPage>();
        Configure<CastlelineEvilViewModel, CastlelineEvilPage>();
        Configure<TurboNinja2DViewModel, TurboNinja2DPage>();
        Configure<StaffOfHellViewModel, StaffOfHellPage>();
        Configure<SciezkaBohateraViewModel, SciezkaBohateraPage>();
        Configure<ZacmienieViewModel, ZacmieniePage>();
        Configure<UploadGameViewModel, UploadGamePage>();
        Configure<Klikacz24HViewModel, Klikacz24HPage>();
        Configure<BlackWhiteJumpViewModel, BlackWhiteJumpPage>();
        Configure<StarmanSystemViewModel, StarmanSystemPage>();
        Configure<ShadowSquadViewModel, ShadowSquadPage>();
    }

    public Type GetPageType(string key)
    {
        Type? pageType;
        lock (_pages)
        {
            if (!_pages.TryGetValue(key, out pageType))
            {
                throw new ArgumentException($"Page not found: {key}. Did you forget to call PageService.Configure?");
            }
        }

        return pageType;
    }

    private void Configure<VM, V>()
        where VM : ObservableObject
        where V : Page
    {
        lock (_pages)
        {
            var key = typeof(VM).FullName!;
            if (_pages.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already configured in PageService");
            }

            var type = typeof(V);
            if (_pages.Any(p => p.Value == type))
            {
                throw new ArgumentException($"This type is already configured with key {_pages.First(p => p.Value == type).Key}");
            }

            _pages.Add(key, type);
        }
    }
}
