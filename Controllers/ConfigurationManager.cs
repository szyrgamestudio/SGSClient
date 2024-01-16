using System.Xml.Linq;

namespace SGSClient.Controllers
{
    public class ConfigurationManager
    {
        private readonly XDocument _configDocument;

        public ConfigurationManager(string configFilePath)
        {
            try
            {
                _configDocument = XDocument.Load(configFilePath);
            }
            catch (Exception ex)
            {
                // Obsługa błędu ładowania pliku konfiguracyjnego
                Console.WriteLine($"Błąd ładowania pliku konfiguracyjnego: {ex.Message}");
                throw;
            }
        }

        private XElement? GetGameElement(string gameName)
        {
            return _configDocument.Descendants("Game")
                                  .FirstOrDefault(e => e.Element("GameName")?.Value == gameName);
        }

        private string GetGameProperty(string gameName, string propertyName)
        {
            try
            {
                XElement? gameElement = GetGameElement(gameName);
                return gameElement?.Element(propertyName)?.Value ?? string.Empty;
            }
            catch (Exception ex)
            {
                // Obsługa błędu podczas pobierania właściwości
                Console.WriteLine($"Błąd podczas pobierania {propertyName}: {ex.Message}");
                throw;
            }
        }

        public string GetGameName(string gameName) => GetGameProperty(gameName, "GameName");
        public string GetGameVersion(string gameName) => GetGameProperty(gameName, "GameVersion");
        public string GetGamePayloadName(string gameName) => GetGameProperty(gameName, "GamePayloadName");
        public string GetGameZipLink(string gameName) => GetGameProperty(gameName, "GameZipLink");
        public string GetGameVersionLink(string gameName) => GetGameProperty(gameName, "GameVersionLink");
    }
}
