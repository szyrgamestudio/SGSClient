using SGSClient.Models;
using SGSClient.ViewModels;
using SGSClient.Core.Database;
using System.Data;

namespace SGSClient.Controllers
{
    public class ConfigurationManagerSQL
    {
        private readonly DbContext _dbContext;

        public ConfigurationManagerSQL(DbContext dbContext)
        {
            _dbContext = dbContext;
        }
        #region Game info
        public async Task<List<GamesViewModel>> LoadGamesFromDatabaseAsync(bool bypassDraftP)
        {
            List<GamesViewModel> gamesList = [];
            var dataSet = await _dbContext.ExecuteQueryAsync(SqlQueries.gamesInfo, bypassDraftP);
            if (dataSet.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    GamesViewModel game = new GamesViewModel(
                        gameId: row["GameId"].ToString(),
                        gameSymbol: row["GameSymbol"].ToString(),
                        gameTitle: row["Title"].ToString(),
                        gamePayloadName: row["PayloadName"].ToString(),
                        gameExeName: row["ExeName"].ToString(),
                        gameZipLink: row["ZipLink"].ToString(),
                        gameVersionLink: row["VersionLink"].ToString(),
                        gameDescription: row["Description"].ToString(),
                        hardwareRequirements: row["HardwareRequirements"].ToString(),
                        otherInformations: row["OtherInformation"].ToString(),
                        gameDeveloper: row["GameDeveloper"].ToString(),
                        logoPath: row["LogoPath"].ToString(),
                        gameType: row["GameType"].ToString(),
                        draftP: row["DraftP"].ToString()
                    );

                    gamesList.Add(game);
                }
                return gamesList;
            }
            else
                return gamesList;
        }
        public async Task<List<GamesViewModel>> LoadFeaturedGamesFromDatabaseAsync(bool bypassDraftP)
        {
            List<GamesViewModel> gamesList = [];
            var dataSet = await _dbContext.ExecuteQueryAsync(SqlQueries.gamesFeaturedInfo, bypassDraftP);
            if (dataSet.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    GamesViewModel game = new GamesViewModel(
                        gameId: row["GameId"].ToString(),
                        gameSymbol: row["GameSymbol"].ToString(),
                        gameTitle: row["Title"].ToString(),
                        gamePayloadName: row["PayloadName"].ToString(),
                        gameExeName: row["ExeName"].ToString(),
                        gameZipLink: row["ZipLink"].ToString(),
                        gameVersionLink: row["VersionLink"].ToString(),
                        gameDescription: row["Description"].ToString(),
                        hardwareRequirements: row["HardwareRequirements"].ToString(),
                        otherInformations: row["OtherInformation"].ToString(),
                        gameDeveloper: row["GameDeveloper"].ToString(),
                        logoPath: row["LogoPath"].ToString(),
                        gameType: row["GameType"].ToString(),
                        draftP: row["DraftP"].ToString()
                    );

                    gamesList.Add(game);
                }
                return gamesList;
            }
            else
                return gamesList;
        }
        public async Task<List<GamesViewModel>> LoadMyGamesFromDatabaseAsync()
        {
            List<GamesViewModel> gamesList = [];
            var dataSet = await _dbContext.ExecuteQueryAsync(SqlQueries.gamesUserInfo, AppSession.CurrentUserSession.UserId);
            if (dataSet.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    GamesViewModel game = new GamesViewModel(
                        gameId: row["GameId"].ToString(),
                        gameSymbol: row["GameSymbol"].ToString(),
                        gameTitle: row["Title"].ToString(),
                        gamePayloadName: row["PayloadName"].ToString(),
                        gameExeName: row["ExeName"].ToString(),
                        gameZipLink: row["ZipLink"].ToString(),
                        gameVersionLink: row["VersionLink"].ToString(),
                        gameDescription: row["Description"].ToString(),
                        hardwareRequirements: row["HardwareRequirements"].ToString(),
                        otherInformations: row["OtherInformation"].ToString(),
                        gameDeveloper: row["GameDeveloper"].ToString(),
                        logoPath: row["LogoPath"].ToString(),
                        gameType: row["GameType"].ToString(),
                        draftP: row["DraftP"].ToString()
                    );

                    gamesList.Add(game);
                }
                return gamesList;
            }
            else
                return gamesList;
        }
        public async Task<List<string>> LoadGalleryImagesFromDatabaseAsync(string gameSymbol)
        {
            List<string> galleryImages = [];
            var dataSet = await _dbContext.ExecuteQueryAsync(SqlQueries.gameImagesSQL, gameSymbol);
            if (dataSet.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in dataSet.Tables[0].Rows)
                    galleryImages.Add(row["ImagePath"].ToString());

                return galleryImages;
            }
            else
                return galleryImages;
        }
        public async Task<string> GetGameVersion(string gameIdentifier)
        {
            List<string> galleryImages = [];
            var dataSet = await _dbContext.ExecuteQueryAsync(SqlQueries.gameCurrentVersionSQL, gameIdentifier);
            if (dataSet.Tables[0].Rows.Count > 0)
                return dataSet.Tables[0].Rows[0]["GameId"].ToString();
            else
                return "0.0.0.0";
        }
        #endregion

        #region Comments
        public async Task<List<Comment>> LoadCommentsFromDatabaseAsync(string gameIdentifier)
        {
            List<Comment> comments = [];
            var dataSet = await _dbContext.ExecuteQueryAsync(SqlQueries.loadCommentsSQL, gameIdentifier);
            if (dataSet.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    comments.Add(new Comment
                    {
                        CommentId = row.Field<int>("Id"),
                        AuthorId = row.Field<int>("DeveloperId"),
                        Author = row.Field<string>("Name"),
                        Content = row.Field<string>("Comment")
                    });
                }

                return comments;
            }
            else
                return comments;
        }
        public async Task AddCommentToDatabaseAsync(string gameIdentifier, Comment comment)
        {
            await _dbContext.ExecuteQueryAsync(SqlQueries.insertCommentSQL, AppSession.CurrentUserSession.UserId, comment.Content, gameIdentifier);
        }
        public async Task UpdateCommentInDatabase(Comment comment)
        {
            await _dbContext.ExecuteQueryAsync(SqlQueries.updateCommentSQL, comment.Content, comment.CommentId);
        }
        public async Task DeleteCommentFromDatabase(Comment comment)
        {
            await _dbContext.ExecuteQueryAsync(SqlQueries.deleteCommentSQL, comment.CommentId);
        }
        #endregion
    }
}
