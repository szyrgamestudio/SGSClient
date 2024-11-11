using CommunityToolkit.Mvvm.ComponentModel;
using SGSClient.Models;
using SGSClient.Core.Database;
using System.Collections.ObjectModel;
using System.Linq;
using SGSClient.Controllers;

namespace SGSClient.ViewModels
{
    public partial class GameBaseViewModel : ObservableRecipient
    {
        private readonly ConfigurationManagerSQL _configManagerSQL;
        private ObservableCollection<GameRating> _allRatings;
        private const int PageSize = 3;

        [ObservableProperty]
        private ObservableCollection<GameRating> ratings;

        [ObservableProperty]
        private int currentPage;
        public bool CanGoToPreviousPage => CurrentPage > 0;
        public bool CanGoToNextPage => (CurrentPage + 1) * PageSize < _allRatings.Count;

        public GameBaseViewModel()
        {
            _configManagerSQL = new ConfigurationManagerSQL(new DbContext());
            _allRatings = new ObservableCollection<GameRating>();

            Ratings = new ObservableCollection<GameRating>();
            CurrentPage = 0;
        }


        public async Task LoadRatings(string gameIdentifier)
        {
            _allRatings.Clear();
            var ratings = await _configManagerSQL.LoadRatingsFromDB(gameIdentifier);
            foreach (var gameRating in ratings)
                _allRatings.Add(gameRating);

            LoadPage(0);
        }


        public async void AddRating(string gameIdentifier, GameRating gameRating)
        {
            await _configManagerSQL.AddRatingToDB(gameIdentifier, gameRating);
            _allRatings.Add(gameRating);
            LoadPage(CurrentPage);
        }
        public async void UpdateRating(GameRating gameRating)
        {
            await _configManagerSQL.UpdateRatingInDB(gameRating);

            var comment = _allRatings.FirstOrDefault(c => c.RatingId == gameRating.RatingId);
            if (comment != null)
            {
                comment.Author = gameRating.Author;
                comment.Review = gameRating.Review;
                LoadPage(CurrentPage); // Refresh the current page
            }
        }
        public async void DeleteRating(GameRating gameRating)
        {
            await _configManagerSQL.DeleteRatingInDB(gameRating);
            _allRatings.Remove(gameRating);
            LoadPage(CurrentPage); // Refresh the current page
        }


        public void LoadPage(int pageNumber)
        {
            Ratings.Clear();
            CurrentPage = pageNumber;
            var ratingsToShow = _allRatings.Skip(CurrentPage * PageSize).Take(PageSize);
            foreach (var gameRating in ratingsToShow)
            {
                Ratings.Add(gameRating);
            }
            OnPropertyChanged(nameof(CanGoToPreviousPage));
            OnPropertyChanged(nameof(CanGoToNextPage));
        }
        public void GoToPreviousPage()
        {
            if (CanGoToPreviousPage)
            {
                LoadPage(CurrentPage - 1);
            }
        }
        public void GoToNextPage()
        {
            if (CanGoToNextPage)
            {
                LoadPage(CurrentPage + 1);
            }
        }
    }
}
