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

    private void ButtonCastlelineEvil_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Frame.Navigate(typeof(CastlelineEvilPage), null, new DrillInNavigationTransitionInfo());
    }

    private void ButtonTurboNinja2D_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Frame.Navigate(typeof(TurboNinja2DPage), null, new DrillInNavigationTransitionInfo());
    }

    private void ButtonSciezkaBohatera_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Frame.Navigate(typeof(SciezkaBohateraPage), null, new DrillInNavigationTransitionInfo());
    }

    //private void ButtonZacmienie_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    //{
    //    Frame.Navigate(typeof(ZacmieniePage), null, new DrillInNavigationTransitionInfo());
    //}
    private void ButtonZacmienie_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        //Przekazujemy identyfikator gry "Zacmienie" do strony DoddaniPage
        Frame.Navigate(typeof(ZacmieniePage), "Zacmienie", new DrillInNavigationTransitionInfo());
    }

    private void ButtonStaffOfHell_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        //Frame.Navigate(typeof(StaffOfHellPage), null, new DrillInNavigationTransitionInfo());
    }

    private void ButtonKlikacz24H_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Frame.Navigate(typeof(Klikacz24HPage), null, new DrillInNavigationTransitionInfo());
    }

    private void ButtonShadowSquad_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Frame.Navigate(typeof(ShadowSquadPage), null, new DrillInNavigationTransitionInfo());
    }

    private void ButtonStarmanSystem_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Frame.Navigate(typeof(StarmanSystemPage), null, new DrillInNavigationTransitionInfo());
    }

    private void ButtonBlackWhiteJump_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Frame.Navigate(typeof(BlackWhiteJumpPage), null, new DrillInNavigationTransitionInfo());
    }

}
