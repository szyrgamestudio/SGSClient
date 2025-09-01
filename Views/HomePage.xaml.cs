using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SGSClient.ViewModels;

namespace SGSClient.Views;
public sealed partial class HomePage : Page
{
    #region Ctor
    public HomePage()
    {
        ViewModel = App.GetService<HomeViewModel>();
        InitializeComponent();
    }
    #endregion

    #region Methods
    private void UpdateScrollButtonsVisibility()
    {
        ViewModel.ScrollForwardButtonVisibility = scroller.ScrollableWidth > 0 ? Visibility.Visible : Visibility.Collapsed;
    }
    #endregion

    #region Properties
    public HomeViewModel ViewModel { get; }
    #endregion

    #region Event Handlers
    private void Scroller_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
    {
        ViewModel.ScrollBackButtonVisibility = e.FinalView.HorizontalOffset < 1 ? Visibility.Collapsed : Visibility.Visible;
        ViewModel.ScrollForwardButtonVisibility = e.FinalView.HorizontalOffset > scroller.ScrollableWidth - 1 ? Visibility.Collapsed : Visibility.Visible;
    }
    private void Scroller_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateScrollButtonsVisibility();
    }
    private void ScrollBackBtn_Click(object sender, RoutedEventArgs e)
    {
        scroller.ChangeView(scroller.HorizontalOffset - scroller.ViewportWidth, null, null);
    }
    private void ScrollForwardBtn_Click(object sender, RoutedEventArgs e)
    {
        scroller.ChangeView(scroller.HorizontalOffset + scroller.ViewportWidth, null, null);
    }
    private void GamesButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.NavigateToGamesCommand.Execute(null);
    }
    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.NavigateToLoginCommand.Execute(null);
    }
    #endregion

}
