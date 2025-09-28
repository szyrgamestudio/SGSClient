using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using SGSClient.ViewModels;

namespace SGSClient.Views;

public sealed partial class HomePage : Page
{
    #region Fields
    public HomeViewModel HomeViewModel { get; }
    #endregion

    #region Ctor
    public HomePage()
    {
        HomeViewModel = App.GetService<HomeViewModel>();
        InitializeComponent();
    }
    #endregion

    #region Methods
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        HomeViewModel.LoadGamesFromDatabase();
    }
    private void GamesScrollViewer_Loaded(object sender, RoutedEventArgs e)
    {
        var sv = (ScrollViewer)sender;
        // rejestracja globalna (zawsze dostaje eventy)
        sv.AddHandler(UIElement.PointerWheelChangedEvent,
                      new PointerEventHandler(GamesScrollViewer_PointerWheelChanged),
                      handledEventsToo: true);
    }
    private void GamesScrollViewer_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        var sv = (ScrollViewer)sender;
        int delta = e.GetCurrentPoint(sv).Properties.MouseWheelDelta;

        // przesuwamy w poziomie
        double step = 80; // piksele na „klik” rolki
        sv.ChangeView(sv.HorizontalOffset - Math.Sign(delta) * step, null, null, true);

        e.Handled = true;
    }
    private void ButtonGame_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button clickedButton)
        {
            string? gameName = clickedButton.Tag?.ToString();
            if (!string.IsNullOrEmpty(gameName))
            {
                Frame.Navigate(typeof(GameBasePage), gameName, new DrillInNavigationTransitionInfo());
            }
        }
    }

    #endregion

}
