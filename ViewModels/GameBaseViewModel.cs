using System.Collections.ObjectModel;
using System.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using SGSClient.Core.Authorization;
using SGSClient.Core.Database;
using SGSClient.Core.Extensions;
using SGSClient.Models;

namespace SGSClient.ViewModels
{
    public partial class GameBaseViewModel : ObservableRecipient
    {
        private readonly IAppUser _appUser;
        private ObservableCollection<GameRating> _allRatings;
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

        public GameBaseViewModel(IAppUser appUser)
        {
            ratingCount = 0;
            avgRating = "5.0";
            count1 = 0;
            count2 = 0;
            count3 = 0;
            count4 = 0;
            count5 = 0;

            _allRatings = new ObservableCollection<GameRating>();

            Ratings = new ObservableCollection<GameRating>();
            CurrentPage = 0;
            _appUser = appUser;
        }
        public bool UserRatingP()
        {
            var dataSet = db.con.select(SqlQueries.userRatingSQL, _appUser.UserId);
            if (dataSet.Tables[0].Rows.Count > 0)
                return true;
            else
                return false;
        }
        public void LoadRatings(string gameIdentifier)
        {
            _allRatings.Clear();
            List<GameRating> gameRatings = [];
            var dataSet = db.con.select(SqlQueries.loadRatingsSQL, gameIdentifier);
            if (dataSet.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    _allRatings.Add(new GameRating
                    {
                        RatingId = row.Field<int>("Id"),
                        UserId = row.Field<int>("DeveloperId"),
                        Author = row.Field<string>("Name"),
                        Rating = row.Field<int>("Rating"),
                        Title = row.Field<string>("Title"),
                        Review = row.Field<string>("Review")
                    });
                }
            }
            LoadPage(0);
        }
        public void LoadGameRatingsStats(string gameIdentifier)
        {
            var dataSet = db.con.select(SqlQueries.loadGameRatingStatsSQL, gameIdentifier);
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
        public DataSet ReturnUserRating(string gameIdentifier)
        {
            return db.con.select(SqlQueries.loadRatingSQL, gameIdentifier, _appUser.UserId);;
        }

        public void AddRating(string gameIdentifier, GameRating gameRating)
        {
            if (gameRating.RatingId > 0)
                UpdateRating(gameRating, gameIdentifier);
            else
                db.con.exec(SqlQueries.insertRatingSQL, gameIdentifier, _appUser.UserId, gameRating.Rating, gameRating.Title, gameRating.Review);

            LoadGameRatingsStats(gameIdentifier);
            LoadPage(CurrentPage);
        }
        public void UpdateRating(GameRating gameRating, string gameIdentifier)
        {
            db.con.exec(SqlQueries.updateRatingSQL, gameRating.RatingId, gameRating.Rating, gameRating.Title, gameRating.Review);
            LoadGameRatingsStats(gameIdentifier);
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
    }
}