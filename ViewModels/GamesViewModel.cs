using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SGSClient.DataAccess.Repositories;
using SGSClient.Models;
namespace SGSClient.ViewModels
{
    public partial class GamesViewModel : ObservableRecipient
    {
        #region Constructors and Properties
        public ObservableCollection<Game> GamesList { get; private set; } = new();
        public ObservableCollection<Game> GamesFeaturedList { get; private set; } = new();
        public GamesViewModel() { }

        #endregion

        #region Public Methods
        public void LoadGamesFromDatabase()
        {
            try
            {
                GamesList = new ObservableCollection<Game>(GamesRepository.FetchGames(false));
                GamesFeaturedList = new ObservableCollection<Game>(GamesRepository.FetchFeaturedGames(false));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading games: {ex.Message}");
            }
        }
        #endregion
    }
}