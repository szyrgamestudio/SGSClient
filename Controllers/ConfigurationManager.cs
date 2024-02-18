using System;
using System.Linq;
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

        public XElement? GetGameElement(string gameName)
        {
            return _configDocument.Descendants("Game")
                                  .FirstOrDefault(e => e.Attribute("name")?.Value == gameName);
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

        public string GetGameTitle(string gameName) => GetGameProperty(gameName, "GameTitle");
        public string GetGameVersion(string gameName) => GetGameProperty(gameName, "GameVersion");
        public string GetGameDeveloper(string gameName) => GetGameProperty(gameName, "GameDeveloper");
        public string GetGamePayloadName(string gameName) => GetGameProperty(gameName, "GamePayloadName");
        public string GetGameExeName(string gameName) => GetGameProperty(gameName, "GameExe");
        public string GetGameZipLink(string gameName) => GetGameProperty(gameName, "GameZipLink");
        public string GetGameVersionLink(string gameName) => GetGameProperty(gameName, "GameVersionLink");
        public string GetGameDescription(string gameName) => GetGameProperty(gameName, "GameDescription");
        public string GetHardwareRequirements(string gameName) => GetGameProperty(gameName, "HardwareRequirements");
        public string GetOtherInformations(string gameName) => GetGameProperty(gameName, "OtherInformation");
    }
}
