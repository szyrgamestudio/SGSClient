using CommunityToolkit.Mvvm.ComponentModel;
using SGSClient.Models;
using SGSClient.Core.Database;
using System.Collections.ObjectModel;
using System.Linq;
using SGSClient.Controllers;
using Windows.UI;
using System.Data;
using System.Diagnostics.Metrics;

namespace SGSClient.ViewModels
{
    public partial class GameBaseViewModel : ObservableRecipient
    {
        private readonly ConfigurationManagerSQL _configManagerSQL;
        private ObservableCollection<GameRating> _allRatings;
        private readonly DbContext _dbContext;
        private const int PageSize = 2;

        [ObservableProperty]
        private ObservableCollection<GameRating> ratings;

        [ObservableProperty]
        private int currentPage;
        public bool CanGoToPreviousPage => CurrentPage > 0;
        public bool CanGoToNextPage => (CurrentPage + 1) * PageSize < _allRatings.Count;

        [ObservableProperty]
        private int ratingCount;

        [ObservableProperty]
        private string avgRating;

        [ObservableProperty]
        private int count1;

        [ObservableProperty]
        private int count2;

        [ObservableProperty]
        private int count3;

        [ObservableProperty]
        private int count4;

        [ObservableProperty]
        private int count5;


        public GameBaseViewModel(DbContext dbContext)
        {
            ratingCount = 0;
            avgRating = "5.0";
            count1 = 0;
            count2 = 0;
            count3 = 0;
            count4 = 0;
            count5 = 0;

            _dbContext = dbContext;

            _configManagerSQL = new ConfigurationManagerSQL(new DbContext());
            _allRatings = new ObservableCollection<GameRating>();

            Ratings = new ObservableCollection<GameRating>();
            CurrentPage = 0;
        }

        public async Task<bool> UserRatingP()
        {
            var dataSet = await _dbContext.ExecuteQueryAsync(SqlQueries.userRatingSQL, AppSession.CurrentUserSession.UserId);
            if (dataSet.Tables[0].Rows.Count > 0)
                return true;
            else
                return false;
        }

        public async Task LoadRatings(string gameIdentifier)
        {
            _allRatings.Clear();
            var ratings = await _configManagerSQL.LoadRatingsFromDB(gameIdentifier);
            foreach (var gameRating in ratings)
                _allRatings.Add(gameRating);

            LoadPage(0);
        }

        public async Task LoadGameRatingsStats(string gameIdentifier)
        {
            var dataSet = await _dbContext.ExecuteQueryAsync(SqlQueries.loadGameRatingStatsSQL, gameIdentifier);
            if (dataSet.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    RatingCount = row.Field<int>("RatingCount");
                    AvgRating = row.Field<string>("AvgRating");
                    Count1 = row.Field<int>("Count1");
                    Count2 = row.Field<int>("Count2");
                    Count3 = row.Field<int>("Count3");
                    Count4 = row.Field<int>("Count4");
                    Count5 = row.Field<int>("Count5");
                }
            }
        }

        public async Task<System.Data.DataSet> ReturnUserRating(string gameIdentifier)
        {
            var dataSet = await _dbContext.ExecuteQueryAsync(SqlQueries.loadRatingSQL, gameIdentifier, AppSession.CurrentUserSession.UserId);
            return dataSet;
        }


        public async void AddRating(string gameIdentifier, GameRating gameRating)
        {
            if (gameRating.RatingId > 0)
                UpdateRating(gameRating, gameIdentifier);
            else
                await _dbContext.ExecuteQueryAsync(SqlQueries.insertRatingSQL, gameIdentifier, AppSession.CurrentUserSession.UserId, gameRating.Rating, gameRating.Title, gameRating.Review);

            await LoadGameRatingsStats(gameIdentifier);
            LoadPage(CurrentPage);
        }
        public async void UpdateRating(GameRating gameRating, string gameIdentifier)
        {
            await _dbContext.ExecuteQueryAsync(SqlQueries.updateRatingSQL, gameRating.RatingId, gameRating.Rating, gameRating.Title, gameRating.Review);
            await LoadGameRatingsStats(gameIdentifier);
            LoadPage(CurrentPage);
        }
        public async void DeleteRating(GameRating gameRating)
        {
            await _dbContext.ExecuteQueryAsync(SqlQueries.deleteRatingSQL, gameRating.RatingId);
            _allRatings.Remove(gameRating);
            LoadPage(CurrentPage);
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
