using System.ComponentModel;

namespace SGSClient.Models
{
    public class GameRating : INotifyPropertyChanged
    {
        private string? _user;
        private int _rating;
        private string? _title;
        private string? _review;

        public int UserId { get; set; }
        public int RatingId { get; set; }

        public string Author
        {
            get => _user;
            set
            {
                if (_user != value)
                {
                    _user = value;
                    OnPropertyChanged(nameof(Author));
                }
            }
        }

        public string Review
        {
            get => _review;
            set
            {
                if (_review != value)
                {
                    _review = value;
                    OnPropertyChanged(nameof(Review));
                }
            }
        }
        public int Rating
        {
            get => _rating;
            set
            {
                if (_rating != value)
                {
                    _rating = value;
                    OnPropertyChanged(nameof(Rating));
                }
            }
        }
        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
