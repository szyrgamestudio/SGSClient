using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;
using SGSClient.Contracts.Services;
using SGSClient.Core.Authorization;
using SGSClient.Core.Utilities.AppInfoUtility.Interfaces;
using SGSClient.DataAccess.Repositories;
using SGSClient.Models;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Text;
using Windows.Storage;

namespace SGSClient.ViewModels;

public partial class HomeViewModel : ObservableRecipient
{
    private readonly IAppUser _appUser;
    private readonly IAppInfo _appInfo;

    private readonly INavigationService _navigationService;
    public ObservableCollection<Game> GamesFeaturedList { get; } = new();

    public HomeViewModel(INavigationService navigationService, IAppUser appUser, IAppInfo appInfo)
    {
        _navigationService = navigationService;
        _appUser = appUser;
        _appInfo = appInfo;
    }

    public async void LoadGamesFromDatabase()
    {
        string nextcloudLogin = _appInfo.GetAppSetting("NextcloudLogin").Value;
        string nextcloudPassword = _appInfo.GetAppSetting("NextcloudPassword").Value;

        try
        {
            var games = GamesRepository.FetchFeaturedGames(false);

            GamesFeaturedList.Clear();
            foreach (var g in games)
                GamesFeaturedList.Add(g);

            await LoadAllLogosAsync(nextcloudLogin, nextcloudPassword);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading games: {ex.Message}");
        }
    }

    private async Task LoadAllLogosAsync(string username, string password)
    {
        var tasks = GamesFeaturedList
            .Where(g => !string.IsNullOrEmpty(g.LogoPath))
            .Select(g => LoadLogoCachedAsync(g, username, password))
            .ToList();

        await Task.WhenAll(tasks);
    }

    private async Task LoadLogoCachedAsync(Game game, string username, string password)
    {
        try
        {
            string fileName = $"{game.GameId}_logo.png";
            var tempFolder = ApplicationData.Current.TemporaryFolder;
            var localFile = await tempFolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);

            bool fileExists = (await localFile.GetBasicPropertiesAsync()).Size > 0;

            if (fileExists)
            {
                using var stream = await localFile.OpenReadAsync();
                var bitmap = new BitmapImage();
                await bitmap.SetSourceAsync(stream);
                game.LogoImage = bitmap;
                return;
            }

            using var client = new HttpClient();
            var authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            var response = await client.GetAsync(game.LogoPath);
            if (response.IsSuccessStatusCode)
            {
                var buffer = await response.Content.ReadAsByteArrayAsync();
                await FileIO.WriteBytesAsync(localFile, buffer);

                using var mem = new MemoryStream(buffer);
                var bitmap = new BitmapImage();
                await bitmap.SetSourceAsync(mem.AsRandomAccessStream());
                game.LogoImage = bitmap;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd ładowania logo {game.GameName}: {ex.Message}");
        }
    }
}
