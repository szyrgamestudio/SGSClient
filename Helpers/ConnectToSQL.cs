using Microsoft.Data.SqlClient;

namespace SGSClient.Helpers
{
    internal class ConnectToSQL
    {
        private readonly string connectionString;

        public ConnectToSQL(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public SqlConnection Connect()
        {
            try
            {
                // Tworzenie obiektu połączenia
                SqlConnection connection = new SqlConnection(connectionString);
                // Otwieranie połączenia
                connection.Open();
                // Zwracanie otwartego połączenia
                return connection;
            }
            catch (Exception ex)
            {
                // Obsługa błędów
                Console.WriteLine("Błąd podczas nawiązywania połączenia z bazą danych: " + ex.Message);
                return null;
            }
        }
    }
}
