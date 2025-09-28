using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SGSClient.Models
{
    public class GameRating : INotifyPropertyChanged
    {
        private string _author;
        private int _rating;
        private string _title;
        private string _review;

        public int UserId { get; set; }
        public int RatingId { get; set; }

        public string Author
        {
            get => _author;
            set => SetField(ref _author, value);
        }

        public string Review
        {
            get => _review;
            set => SetField(ref _review, value);
        }

        public int Rating
        {
            get => _rating;
            set => SetField(ref _rating, value);
        }

        public string Title
        {
            get => _title;
            set => SetField(ref _title, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
