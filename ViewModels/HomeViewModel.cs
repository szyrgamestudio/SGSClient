using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using SGSClient.Contracts.Services;
using SGSClient.DataAccess.Repositories;
using SGSClient.Models;
using System.Collections.ObjectModel;

namespace SGSClient.ViewModels;

public class HomeViewModel : ObservableRecipient
{
    #region Fields
    private Visibility _scrollBackButtonVisibility;
    private Visibility _scrollForwardButtonVisibility;
    private readonly INavigationService _navigationService;
    public ObservableCollection<Game> GamesFeaturedList { get; private set; } = new();

    #endregion

    #region Ctor
    public HomeViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }
    #endregion

    #region Methods
    public void LoadGamesFromDatabase()
    {
        try
        {
            var games = GamesRepository.FetchFeaturedGames(false);

            GamesFeaturedList.Clear();
            foreach (var g in games)
                GamesFeaturedList.Add(g);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading games: {ex.Message}");
        }
    }
    #endregion

}
