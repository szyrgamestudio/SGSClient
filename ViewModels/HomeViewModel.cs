using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using SGSClient.Contracts.Services;

namespace SGSClient.ViewModels;
public class HomeViewModel : ObservableRecipient
{
    #region Fields
    private Visibility _scrollBackButtonVisibility;
    private Visibility _scrollForwardButtonVisibility;
    private readonly INavigationService _navigationService;
    #endregion

    #region Ctor
    public HomeViewModel(INavigationService navigationService)
    {
        ScrollBackButtonVisibility = Visibility.Collapsed;
        ScrollForwardButtonVisibility = Visibility.Collapsed;

        _navigationService = navigationService;
        NavigateToGamesCommand = new RelayCommand(NavigateToGames);
        NavigateToLoginCommand = new RelayCommand(NavigateToLogin);
    }
    #endregion

    #region Methods
    private void NavigateToGames()
    {
        _navigationService.NavigateTo(typeof(GamesViewModel).FullName!);
    }
    private void NavigateToLogin()
    {
        //_navigationService.NavigateTo(typeof(LoginViewModel).FullName!);
    }
    #endregion

    #region Properties
    public Visibility ScrollBackButtonVisibility
    {
        get => _scrollBackButtonVisibility;
        set => SetProperty(ref _scrollBackButtonVisibility, value);
    }
    public Visibility ScrollForwardButtonVisibility
    {
        get => _scrollForwardButtonVisibility;
        set => SetProperty(ref _scrollForwardButtonVisibility, value);
    }

    public ICommand NavigateToGamesCommand { get; }
    public ICommand NavigateToLoginCommand { get; }
    #endregion
}
