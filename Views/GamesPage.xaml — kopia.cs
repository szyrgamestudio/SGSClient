using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using SGSClient.Controllers;
using SGSClient.Helpers;
using SGSClient.ViewModels;
using System.Windows.Documents.DocumentStructures;
using System.Xml;
using Windows.Gaming.Input;
namespace SGSClient.Views;

public sealed partial class GamesPage : Page
{
    private readonly ConfigurationManager configManager;

    public GamesViewModel ViewModel { get; }
    private const string ConnectionString = @"Data Source=(localdb)\localDB1;Initial Catalog=SGS_SGSCLIENT;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

    public GamesPage()
    {
        this.InitializeComponent();
        configManager = new ConfigurationManager("D:\\DEVELOPMENT\\Repozytoria\\SGSClient\\Config\\appconfig.xml");
        LoadGamesFromXml();

        ConnectToSQL connector = new ConnectToSQL(ConnectionString);
        connector.Connect(); // Zakładając, że Connect to metoda w klasie ConnectToSQL
        // Reszta logiki aplikacji
    }

    private void LoadGamesFromXml()
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load("D:\\DEVELOPMENT\\Repozytoria\\SGSClient\\Config\\appconfig.xml");

        List<GamesViewModel> gamesList = new List<GamesViewModel>();

        foreach (XmlNode node in xmlDoc.SelectNodes("//Game"))
        {
            if (node is XmlElement element)
            {
                gamesList.Add(new GamesViewModel(element));
            }
        }

        GamesItemsControl.ItemsSource = gamesList;
    }
    private void ButtonGame_Click(object sender, RoutedEventArgs e)
    {
        // Tutaj umieść kod obsługujący kliknięcie przycisku
        // Możesz używać obiektu sender do uzyskania dostępu do informacji na temat klikniętego przycisku

        // Przykład użycia sender:
        if (sender is Button clickedButton)
        {
            string gameName = clickedButton.Tag?.ToString();
            if (!string.IsNullOrEmpty(gameName))
            {
                Frame.Navigate(typeof(GameBasePage), gameName, new DrillInNavigationTransitionInfo());
            }
        }
    }

    private void ButtonAtorth_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Frame.Navigate(typeof(GameBasePage), "AtorthTalesOfUlkimond", new DrillInNavigationTransitionInfo());
    }

    private void ButtonDoddani_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Frame.Navigate(typeof(GameBasePage), "Doddani", new DrillInNavigationTransitionInfo());
    }

}
