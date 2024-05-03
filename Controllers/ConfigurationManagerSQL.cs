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

        public List<GamesViewModel> LoadGamesFromDatabase()
        {
            List<GamesViewModel> gamesList = new List<GamesViewModel>();

            string query = @"
select
  g.Id
, g.Title
, g.Symbol   [GameSymbol]
, d.Name     [GameDeveloper]
, l.LogoPath [LogoPath]
, g.PayloadName
, g.ExeName
, g.ZipLink
, g.VersionLink
, g.Description
, g.HardwareRequirements
, g.OtherInformation
from sgsGames g
inner join sgsDevelopers d on d.Id = g.DeveloperId
left join sgsGameLogo l on l.GameId = g.Id
where g.DraftP = 0
";

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            GamesViewModel game = new GamesViewModel(
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
                                logoPath: reader["LogoPath"].ToString()
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
        public string GetGameVersion(string gameIdentifier)
        {
            string query = @"
        SELECT VersionLink
        FROM sgsGames
        WHERE Symbol = @GameIdentifier
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
                            string versionLink = result.ToString();

                            // Pobierz zawartość pliku tekstowego z linku
                            string versionContent = httpClient.GetStringAsync(versionLink).Result;

                            // Zwróć zawartość pliku jako wersję gry
                            return versionContent.Trim();
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
