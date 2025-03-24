using SQLite;

namespace SGSClient.ViewModels
{
    public class GameVersion
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Identifier { get; set; }
        public string Version { get; set; }
    }
}