﻿using CommunityToolkit.Mvvm.ComponentModel;
using SGSClient.Models;
using SGSClient.Core.Database;
using System.Collections.ObjectModel;
using SGSClient.Controllers;

namespace SGSClient.ViewModels
{
    public partial class GameBaseViewModel : ObservableRecipient
    {
        private readonly ConfigurationManagerSQL _configManagerSQL;
        private ObservableCollection<Comment> _comments;

        public ObservableCollection<Comment> Comments
        {
            get => _comments;
            set
            {
                SetProperty(ref _comments, value);
            }
        }

        public GameBaseViewModel(ConfigurationManagerSQL configManagerSQL)
        {
            _configManagerSQL = configManagerSQL;
            Comments = new ObservableCollection<Comment>();
        }

        public void UpdateComment(Comment updatedComment)
        {
            // Update comment in the database
            _configManagerSQL.UpdateCommentInDatabase(updatedComment);

            // Update comment in the collection
            var comment = Comments.FirstOrDefault(c => c.CommentId == updatedComment.CommentId);
            if (comment != null)
            {
                comment.Author = updatedComment.Author;
                comment.Content = updatedComment.Content;
            }
        }

        public void LoadComments(string gameIdentifier)
        {
            Comments.Clear();
            var commentsFromDb = _configManagerSQL.LoadCommentsFromDatabase(gameIdentifier);
            foreach (var comment in commentsFromDb)
            {
                Comments.Add(comment);
            }
        }

        public void DeleteComment(Comment commentToDelete)
        {
            // Implement deletion logic
            _configManagerSQL.DeleteCommentFromDatabase(commentToDelete);
            Comments.Remove(commentToDelete);
        }
    }
}
