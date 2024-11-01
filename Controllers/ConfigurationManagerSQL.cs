using SGSClient.Models;
using SGSClient.ViewModels;
using Microsoft.Data.SqlClient;
using SGSClient.Core.Database;
using System.Data;

namespace SGSClient.Controllers
{
    public class ConfigurationManagerSQL
    {
        private readonly string _connectionString;
        public ConfigurationManagerSQL(string connectionString)
        {
            _connectionString = connectionString;
        }
        public async Task<List<GamesViewModel>> LoadGamesFromDatabaseAsync(bool bypassDraftP)
        {
            List<GamesViewModel> gamesList = [];

            string query = @"
select
  g.Id       [GameId]
, g.Title
, g.Symbol   [GameSymbol]
, d.Name     [GameDeveloper]
, l.LogoPath [LogoPath]
, t.Name	 [GameType]
, g.PayloadName
, g.ExeName
, g.ZipLink
, g.VersionLink
, g.Description
, g.HardwareRequirements
, g.OtherInformation
, g.DraftP
from sgsGames g
inner join sgsDevelopers d on d.Id = g.DeveloperId
left join sgsGameLogo l on l.GameId = g.Id
left join sgsGameTypes t on t.Id = g.TypeId
where g.DraftP = 0 and @p0 = 0 or @p0 = 1
order by g.Title";

            try
            {
                using SqlConnection connection = db.Connect();
                DataSet result = await db.SelectSQLAsync(connection, query, bypassDraftP);

                if (result.Tables.Count > 0)
                {
                    foreach (DataRow row in result.Tables[0].Rows)
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
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading games from the database: {ex.Message}");
                throw;
            }

            return gamesList;
        }
        public async Task<List<GamesViewModel>> LoadFeaturedGamesFromDatabaseAsync(bool bypassDraftP)
        {
            List<GamesViewModel> gamesList = new List<GamesViewModel>();

            string query = @"
    select
      g.Id       [GameId]
    , g.Title
    , g.Symbol   [GameSymbol]
    , d.Name     [GameDeveloper]
    , l.LogoPath [LogoPath]
    , t.Name	 [GameType]
    , g.PayloadName
    , g.ExeName
    , g.ZipLink
    , g.VersionLink
    , g.Description
    , g.HardwareRequirements
    , g.OtherInformation
    , g.DraftP
    from sgsGames g
    inner join sgsDevelopers d on d.Id = g.DeveloperId
    left join sgsGameLogo l on l.GameId = g.Id
    left join sgsGameTypes t on t.Id = g.TypeId
    where (g.DraftP = 0 and @p0 = 0 or @p0 = 1) and g.FeaturedP = 1
    order by g.Title
    ";

            try
            {
                using (SqlConnection connection = db.Connect())
                {
                    DataSet result = await db.SelectSQLAsync(connection, query, bypassDraftP);
                    if (result.Tables.Count > 0)
                    {
                        foreach (DataRow row in result.Tables[0].Rows)
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
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading featured games from the database: {ex.Message}");
                throw;
            }

            return gamesList;
        }
        public async Task<List<GamesViewModel>> LoadMyGamesFromDatabaseAsync()
        {
            List<GamesViewModel> gamesList = new List<GamesViewModel>();

            string query = @"
select  
  g.Id       [GameId]
, g.Title
, g.Symbol   [GameSymbol]
, d.Name     [GameDeveloper]
, l.LogoPath [LogoPath]
, t.Name     [GameType]
, g.PayloadName
, g.ExeName
, g.ZipLink
, g.VersionLink
, g.Description
, g.HardwareRequirements
, g.OtherInformation
, g.DraftP
from Registration r
inner join sgsDevelopers d on d.Id = r.DeveloperId
inner join sgsGames g on g.DeveloperId = d.Id
left join sgsGameLogo l on l.GameId = g.Id
left join sgsGameTypes t on t.Id = g.TypeId
where r.id = @userId/* and g.DraftP = 0*/
";

            // Use the connection directly from db class
            using SqlConnection connection = db.Connect();
            try
            {
                Console.WriteLine($"Connection state before opening: {connection.State}");

                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                    Console.WriteLine("Connection opened.");
                }

                object[] parameters = { AppSession.CurrentUserSession.UserId };
                DataSet result;

                try
                {
                    // Call the asynchronous method to get results
                    result = await db.SelectSQLAsync(connection, query, parameters);
                }
                catch (Exception sqlEx)
                {
                    Console.WriteLine($"Error executing SQL command: {sqlEx.Message}");
                    return gamesList; // Return empty list on error
                }

                // Check if any tables were returned and process the results
                if (result.Tables.Count > 0 && result.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in result.Tables[0].Rows)
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
                }
                else
                {
                    Console.WriteLine($"No games found for user ID: {AppSession.CurrentUserSession.UserId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading games from database: {ex.Message}");
            }

            return gamesList;
        }
        public async Task<List<string>> LoadGalleryImagesFromDatabaseAsync(string gameSymbol)
        {
            List<string> galleryImages = new List<string>();
            string query = @"
select
  i.ImagePath
from sgsGameImages i
inner join sgsGames g on g.Id = i.GameId
where g.Symbol = @GameSymbol
";

            try
            {
                using (var connection = new SqlConnection(db.Config.GetConnectionString())) // Use the connection string from the db class
                {
                    await connection.OpenAsync(); // Open connection asynchronously

                    // Use the db class to create the command
                    var command = db.CommandSQL(connection, query);
                    command.Parameters.AddWithValue("@GameSymbol", gameSymbol); // Add parameter

                    // Use SelectSQLAsync to execute the command
                    var dataset = await db.SelectSQLAsync(command);

                    // Assuming the DataSet contains one table and each row has the ImagePath
                    foreach (DataRow row in dataset.Tables[0].Rows)
                    {
                        galleryImages.Add(row["ImagePath"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle error during database communication
                Console.WriteLine($"Error loading gallery images from database: {ex.Message}");
                throw;
            }

            return galleryImages;
        }
        public async Task<List<Comment>> LoadCommentsFromDatabaseAsync(string gameIdentifier)
        {
            List<Comment> comments = new List<Comment>();
            var query = @"
select
  c.Id,
  d.Id as DeveloperId,
  d.Name,
  c.Comment
from sgsGameComments c
inner join Registration r on r.Id = c.AuthorId
inner join sgsDevelopers d on d.Id = r.DeveloperId
inner join sgsGames g on g.Id = c.GameId
where g.Symbol = @GameIdentifier
order by c.Id desc
";

            try
            {
                using (var connection = new SqlConnection(db.Config.GetConnectionString())) // Use the connection string from the db class
                {
                    await connection.OpenAsync(); // Open connection asynchronously

                    // Use the db class to create the command
                    var command = db.CommandSQL(connection, query);
                    command.Parameters.AddWithValue("@GameIdentifier", gameIdentifier); // Add parameter

                    // Use SelectSQLAsync to execute the command
                    var dataset = await db.SelectSQLAsync(command);

                    // Assuming the DataSet contains one table and extracting comments from the first table
                    foreach (DataRow row in dataset.Tables[0].Rows)
                    {
                        comments.Add(new Comment
                        {
                            CommentId = row.Field<int>("Id"),
                            AuthorId = row.Field<int>("DeveloperId"),
                            Author = row.Field<string>("Name"),
                            Content = row.Field<string>("Comment")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading comments from database: {ex.Message}"); // Handle error
                throw; // Optionally rethrow or handle as needed
            }

            return comments;
        }


        public void AddCommentToDatabase(string gameIdentifier, Comment comment)
        {
            var query = $@"
insert into sgsGameComments(GameId, AuthorId, Comment, CreationDateTime, ModificationDateTime)
select
  g.Id
, @userId
, @Content
, GETDATE()
, GETDATE()
from sgsGames g
where g.Symbol = '{gameIdentifier}'";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", AppSession.CurrentUserSession.UserId);
                        command.Parameters.AddWithValue("@Content", comment.Content);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }
        }
        public void UpdateCommentInDatabase(Comment comment)
        {
            var query = @"
update c set
  c.Comment = @Content
from sgsGameComments c
where c.Id = @CommentId";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Content", comment.Content);
                        command.Parameters.AddWithValue("@CommentId", comment.CommentId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }
        }
        public void DeleteCommentFromDatabase(Comment comment)
        {
            var query = @"
delete from sgsGameComments
where Id = @CommentId";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CommentId", comment.CommentId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }
        }
        public string GetGameVersion(string gameIdentifier)
        {
            string query = @"
select
  g.CurrentVersion
from sgsGames g
where g.Symbol = @GameIdentifier
";

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@GameIdentifier", gameIdentifier);

                        object result = command.ExecuteScalar();

                        if (result != null)
                        {
                            string versionContent = result.ToString();
                            return versionContent;
                        }
                        else
                        {
                            // Jeśli nie ma linku do wersji dla danej gry, zwróć domyślną wartość
                            return "0.0.0.0";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Obsługa błędu podczas komunikacji z bazą danych lub pobierania zawartości pliku
                Console.WriteLine($"Błąd podczas pobierania wersji gry z bazy danych lub pliku: {ex.Message}");
                throw;
            }
        }
    }
}
