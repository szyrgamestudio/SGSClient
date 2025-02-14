using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SGSClient.ViewModels;

namespace SGSClient.Views;
public sealed partial class MyAccountPage : Page
{
    public MyAccountViewModel ViewModel { get; }
    public MyAccountPage()
    {
        ViewModel = App.GetService<MyAccountViewModel>();
        InitializeComponent();
    }

    #region Navigation
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        DataContext = ViewModel;
        ViewModel.LoadUserData();
    }
    #endregion

    #region UI Event Handlers
    private void ScrollBackBtn_Click(object sender, RoutedEventArgs e)
    {
        scroller.ChangeView(scroller.HorizontalOffset - scroller.ViewportWidth, null, null);
    }

    private void ScrollForwardBtn_Click(object sender, RoutedEventArgs e)
    {
        scroller.ChangeView(scroller.HorizontalOffset + scroller.ViewportWidth, null, null);
    }

    private void Scroller_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
    {
        UpdateScrollButtonsVisibility(e);
    }

    private void Scroller_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateScrollButtonsVisibility();
    }

    private void UploadButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.NavigateToUpload();
    }

    private void MyGamesButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.NavigateToMyGames();
    }
    #endregion

    #region Helper Methods
    private void UpdateScrollButtonsVisibility(ScrollViewerViewChangingEventArgs e)
    {
        ScrollBackBtn.Visibility = e.FinalView.HorizontalOffset < 1 ? Visibility.Collapsed : Visibility.Visible;
        ScrollForwardBtn.Visibility = e.FinalView.HorizontalOffset > scroller.ScrollableWidth - 1 ? Visibility.Collapsed : Visibility.Visible;
    }

    private void UpdateScrollButtonsVisibility()
    {
        ScrollForwardBtn.Visibility = scroller.ScrollableWidth > 0 ? Visibility.Visible : Visibility.Collapsed;
    }
    #endregion
}
