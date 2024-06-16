using SGSClient.Core.Authorization;
using SGSClient.Models;
using SGSClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace SGSClient.Controllers
{
    public class ConfigurationManagerSQL
    {
        private readonly string _connectionString;
        private readonly HttpClient httpClient = new();

        public ConfigurationManagerSQL(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<GamesViewModel> LoadGamesFromDatabase(bool bypassDraftP)
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
where g.DraftP = 0 and @bypassDraftP = 0 or @bypassDraftP = 1
order by g.Title
";

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@bypassDraftP", bypassDraftP);
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            GamesViewModel game = new GamesViewModel(
                                gameId: reader["GameId"].ToString(),
                                gameSymbol: reader["GameSymbol"].ToString(),
                                gameTitle: reader["Title"].ToString(),
                                gamePayloadName: reader["PayloadName"].ToString(),
                                gameExeName: reader["ExeName"].ToString(),
                                gameZipLink: reader["ZipLink"].ToString(),
                                gameVersionLink: reader["VersionLink"].ToString(),
                                gameDescription: reader["Description"].ToString(),
                                hardwareRequirements: reader["HardwareRequirements"].ToString(),
                                otherInformations: reader["OtherInformation"].ToString(),
                                gameDeveloper: reader["GameDeveloper"].ToString(),
                                logoPath: reader["LogoPath"].ToString(),
                                gameType: reader["GameType"].ToString(),
                                draftP: reader["DraftP"].ToString()
                            );

                            gamesList.Add(game);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Obsługa błędu podczas komunikacji z bazą danych
                Console.WriteLine($"Błąd podczas ładowania gier z bazy danych: {ex.Message}");
                throw;
            }

            return gamesList;
        }
        public List<GamesViewModel> LoadMyGamesFromDatabase()
        {
            List<GamesViewModel> gamesList = new List<GamesViewModel>();

            string query = @"
select  
  g.Id       [gameId]
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
from Registration r
inner join sgsDevelopers d on d.Id = r.DeveloperId
inner join sgsGames g on g.DeveloperId = d.Id
left join sgsGameLogo l on l.GameId = g.Id
left join sgsGameTypes t on t.Id = g.TypeId
where r.id = @userId/* and g.DraftP = 0*/
";

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", AppSession.CurrentUserSession.UserId);
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            GamesViewModel game = new GamesViewModel(
                                gameId: reader["GameId"].ToString(),
                                gameSymbol: reader["GameSymbol"].ToString(),
                                gameTitle: reader["Title"].ToString(),
                                gamePayloadName: reader["PayloadName"].ToString(),
                                gameExeName: reader["ExeName"].ToString(),
                                gameZipLink: reader["ZipLink"].ToString(),
                                gameVersionLink: reader["VersionLink"].ToString(),
                                gameDescription: reader["Description"].ToString(),
                                hardwareRequirements: reader["HardwareRequirements"].ToString(),
                                otherInformations: reader["OtherInformation"].ToString(),
                                gameDeveloper: reader["GameDeveloper"].ToString(),
                                logoPath: reader["LogoPath"].ToString(),
                                gameType: reader["GameType"].ToString(),
                                draftP: reader["DraftP"].ToString()
                            );

                            gamesList.Add(game);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Obsługa błędu podczas komunikacji z bazą danych
                Console.WriteLine($"Błąd podczas ładowania gier z bazy danych: {ex.Message}");
                throw;
            }

            return gamesList;
        }
        public List<string> LoadGalleryImagesFromDatabase(string gameSymbol)
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
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@GameSymbol", gameSymbol);
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            string imagePath = reader["ImagePath"].ToString();
                            galleryImages.Add(imagePath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Obsługa błędu podczas komunikacji z bazą danych
                Console.WriteLine($"Błąd podczas ładowania obrazów galerii z bazy danych: {ex.Message}");
                throw;
            }

            return galleryImages;
        }
        public List<Comment> LoadCommentsFromDatabase(string gameIdentifier)
        {
            List<Comment> comments = new List<Comment>();
            var query = $@"
select
  c.Id
, d.Id [DeveloperId]
, d.Name
, c.Comment
from sgsGameComments c
inner join sgsDevelopers d on d.Id = c.AuthorId
inner join sgsGames g on g.Id = c.GameId
--where c.GameId = @GameIdentifier
where Symbol = '{gameIdentifier}'
";
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@GameIdentifier", gameIdentifier);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                comments.Add(new Comment
                                {
                                    CommentId = reader.GetInt32(0),
                                    AuthorId = reader.GetInt32(1),
                                    Author = reader.GetString(2),
                                    Content = reader.GetString(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return comments;
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
