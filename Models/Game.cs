namespace SGSClient.Models
{
    public class Game
    {
        public int GameId { get; set; }
        public string? GameSymbol { get; set; }
        public string? GameName { get; set; }
        public string? GameDeveloper { get; set; }
        public string? GameTitle { get; set; }
        public Uri? ImageSource { get; set; }
        public string? GameVersion { get; set; }
        public string? GamePayloadName { get; set; }
        public string? GameExeName { get; set; }
        public string? GameZipLink { get; set; }
        public string? GameVersionLink { get; set; }
        public string? GameDescription { get; set; }
        public string? HardwareRequirements { get; set; }
        public string? OtherInformations { get; set; }
        public string? LogoPath { get; set; }
        public string? GameType { get; set; }
        public bool DraftP { get; set; }
    }
}