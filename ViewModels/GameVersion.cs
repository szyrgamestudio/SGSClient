using SQLite;

namespace SGSClient.ViewModels
{
    public class GameVersion
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Path { get; set; } = string.Empty;
        public string Identifier { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Exe { get; set; } = string.Empty;
    }
}