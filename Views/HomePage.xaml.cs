using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using CommunityToolkit.WinUI.UI.Animations;
using SGSClient.ViewModels;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;

namespace SGSClient.Views;

public sealed partial class HomePage : Page
{
    private bool isFirstRun = true;
    public HomeViewModel ViewModel
    {
        get;
    }

    public HomePage()
    {
        ViewModel = App.GetService<HomeViewModel>();
        InitializeComponent();
    }


        private void scroller_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
    {
        if (e.FinalView.HorizontalOffset < 1)
        {
            ScrollBackBtn.Visibility = Visibility.Collapsed;
        }
        else if (e.FinalView.HorizontalOffset > 1)
        {
            ScrollBackBtn.Visibility = Visibility.Visible;
        }

        if (e.FinalView.HorizontalOffset > scroller.ScrollableWidth - 1)
        {
            ScrollForwardBtn.Visibility = Visibility.Collapsed;
        }
        else if (e.FinalView.HorizontalOffset < scroller.ScrollableWidth - 1)
        {
            ScrollForwardBtn.Visibility = Visibility.Visible;
        }
    }
    private void ScrollBackBtn_Click(object sender, RoutedEventArgs e)
    {
        scroller.ChangeView(scroller.HorizontalOffset - scroller.ViewportWidth, null, null);
    }
    private void ScrollForwardBtn_Click(object sender, RoutedEventArgs e)
    {
        scroller.ChangeView(scroller.HorizontalOffset + scroller.ViewportWidth, null, null);
    }
    private void scroller_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateScrollButtonsVisibility();
    }
    private void UpdateScrollButtonsVisibility()
    {
        if (scroller.ScrollableWidth > 0)
        {
            ScrollForwardBtn.Visibility = Visibility.Visible;
        }
        else
        {
            ScrollForwardBtn.Visibility = Visibility.Collapsed;
        }
    }

    private void GamesButton_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(GamesPage), null, new DrillInNavigationTransitionInfo());
    }

    private void UploadButton_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(UploadGamePage), null, new DrillInNavigationTransitionInfo());
    }
}
