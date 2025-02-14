using System.ComponentModel;

namespace SGSClient.Models
{
    public class Comment : INotifyPropertyChanged
    {
        private string? _author;
        private string? _content;

        public int CommentId { get; set; }
        public int AuthorId { get; set; }

        public string Author
        {
            get => _author;

            set
            {
                if (_author != value)
                {
                    _author = value;
                    OnPropertyChanged(nameof(Author));
                }
            }
        }
        public string Content
        {
            get => _content;
            set
            {
                if (_content != value)
                {
                    _content = value;
                    OnPropertyChanged(nameof(Content));
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
