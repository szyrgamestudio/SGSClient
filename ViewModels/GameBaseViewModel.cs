using CommunityToolkit.Mvvm.ComponentModel;
using SGSClient.Models;
using SGSClient.Core.Database;
using System.Collections.ObjectModel;
using SGSClient.Controllers;

namespace SGSClient.ViewModels
{
    public partial class GameBaseViewModel : ObservableRecipient
    {
        private readonly ConfigurationManagerSQL _configManagerSQL;

        public GameBaseViewModel(ConfigurationManagerSQL configManagerSQL)
        {
            _configManagerSQL = configManagerSQL;
            Comments = new ObservableCollection<Comment>();
        }

        public ObservableCollection<Comment> Comments { get; }

        public void LoadComments(string gameIdentifier)
        {
            var comments = _configManagerSQL.LoadCommentsFromDatabase(gameIdentifier);
            Comments.Clear();
            foreach (var comment in comments)
            {
                Comments.Add(comment);
            }
        }
    }
}
