using CommunityToolkit.Mvvm.ComponentModel;
using System.Xml;

namespace SGSClient.ViewModels;

public class GamesViewModel : ObservableRecipient
{
    public string GameName { get; set; }
    public string GameDeveloper { get; set; }
    public string GameTitle { get; set; }

    public Uri ImageSource { get; set; }

    public GamesViewModel(XmlElement gameElement)
    {
        // Tutaj zainicjuj właściwości na podstawie danych z XmlElement
        GameName = gameElement.GetAttribute("name");

        string imagePath = $"D:/DEVELOPMENT/Repozytoria/SGSClient/Assets/Games/{GameName.Replace(" ", "")}/Logo.png";
        ImageSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);

        XmlNode developerNode = gameElement.SelectSingleNode("GameDeveloper");
        GameDeveloper = (developerNode != null) ? developerNode.InnerText : "Nieznane";

        XmlNode titleNode = gameElement.SelectSingleNode("GameTitle");
        GameTitle = (titleNode != null) ? titleNode.InnerText : "Nieznane";
    }

}
