using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SGSClient.ViewModels;

namespace SGSClient.Views;

public sealed partial class GamesPage : Page
{
    public GamesViewModel ViewModel
    {
        get;
    }

    public GamesPage()
    {
        ViewModel = App.GetService<GamesViewModel>();
        InitializeComponent();
    }

    private void ButtonAtorth_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Frame.Navigate(typeof(Atorth_TalesOfUlkimondPage), null, new DrillInNavigationTransitionInfo());
    }

    private void ButtonDoddani_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Frame.Navigate(typeof(DoddaniPage), null, new DrillInNavigationTransitionInfo());
    }
}
