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
        private ObservableCollection<Comment> _allComments;
        private const int PageSize = 10;

        [ObservableProperty]
        private ObservableCollection<Comment> comments;

        [ObservableProperty]
        private int currentPage;

        public GameBaseViewModel(ConfigurationManagerSQL configManagerSQL)
        {
            _configManagerSQL = configManagerSQL;
            _allComments = new ObservableCollection<Comment>();
            Comments = new ObservableCollection<Comment>();
            CurrentPage = 0;
        }

        public bool CanGoToPreviousPage => CurrentPage > 0;

        public bool CanGoToNextPage => (CurrentPage + 1) * PageSize < _allComments.Count;

        public void AddComment(string gameIdentifier, Comment newComment)
        {
            _configManagerSQL.AddCommentToDatabase(gameIdentifier, newComment);
            _allComments.Add(newComment);
            LoadPage(CurrentPage);
        }

        public void UpdateComment(Comment updatedComment)
        {
            _configManagerSQL.UpdateCommentInDatabase(updatedComment);

            var comment = _allComments.FirstOrDefault(c => c.CommentId == updatedComment.CommentId);
            if (comment != null)
            {
                comment.Author = updatedComment.Author;
                comment.Content = updatedComment.Content;
                LoadPage(CurrentPage); // Refresh the current page
            }
        }

        public void LoadComments(string gameIdentifier)
        {
            _allComments.Clear();
            var commentsFromDb = _configManagerSQL.LoadCommentsFromDatabase(gameIdentifier);
            foreach (var comment in commentsFromDb)
            {
                _allComments.Add(comment);
            }
            LoadPage(0);
        }

        public void LoadPage(int pageNumber)
        {
            Comments.Clear();
            CurrentPage = pageNumber;
            var commentsToShow = _allComments.Skip(CurrentPage * PageSize).Take(PageSize);
            foreach (var comment in commentsToShow)
            {
                Comments.Add(comment);
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

        public void DeleteComment(Comment commentToDelete)
        {
            _configManagerSQL.DeleteCommentFromDatabase(commentToDelete);
            _allComments.Remove(commentToDelete);
            LoadPage(CurrentPage); // Refresh the current page
        }
    }
}
