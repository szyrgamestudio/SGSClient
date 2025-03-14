﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

using SGSClient.Contracts.Services;
using SGSClient.Helpers;
using SGSClient.ViewModels;

using Windows.System;

namespace SGSClient.Views;

// TODO: Update NavigationViewItem titles and icons in ShellPage.xaml.
public sealed partial class ShellPage : Page
{
    public ShellViewModel ViewModel
    {
        get;
    }

    public ShellPage(ShellViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();

        ViewModel.NavigationService.Frame = NavigationFrame;
        ViewModel.NavigationViewService.Initialize(NavigationViewControl);

        // TODO: Set the title bar icon by updating /Assets/WindowIcon.ico.
        // A custom title bar is required for full window theme and Mica support.
        // https://docs.microsoft.com/windows/apps/develop/title-bar?tabs=winui3#full-customization
        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(AppTitleBar);
        App.MainWindow.Activated += MainWindow_Activated;
        AppTitleBarText.Text = "AppDisplayName".GetLocalized();
        this.PointerPressed += ShellPage_PointerPressed;
    }

    private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        TitleBarHelper.UpdateTitleBar(RequestedTheme);

        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu));
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.GoBack));
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        App.AppTitlebar = AppTitleBarText as UIElement;
    }

    private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        AppTitleBar.Margin = new Thickness()
        {
            Left = sender.CompactPaneLength * (sender.DisplayMode == NavigationViewDisplayMode.Minimal ? 2 : 1),
            Top = AppTitleBar.Margin.Top,
            Right = AppTitleBar.Margin.Right,
            Bottom = AppTitleBar.Margin.Bottom
        };
    }

    private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
    {
        var keyboardAccelerator = new KeyboardAccelerator() { Key = key };

        if (modifiers.HasValue)
        {
            keyboardAccelerator.Modifiers = modifiers.Value;
        }

        keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;

        return keyboardAccelerator;
    }

    private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        var navigationService = App.GetService<INavigationService>();

        var result = navigationService.GoBack();

        args.Handled = result;
    }

    private void ShellPage_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var properties = e.GetCurrentPoint(this).Properties;

        // Check if the XButton1 (back) or XButton2 (forward) is pressed
        if (properties.IsXButton1Pressed)
        {
            // Handle the back navigation
            ViewModel.NavigationService.GoBack();
        }
        else if (properties.IsXButton2Pressed)
        {
            // Handle the forward navigation (if applicable)
            // You may need to implement a forward navigation method in your ViewModel
            // ViewModel.NavigationService.GoForward();
        }
    }

    private void UserAccountItem_Tapped(object sender, TappedRoutedEventArgs e)
    {
        var flyout = new MenuFlyout();

        if (ViewModel.IsUserLoggedIn)
        {
            flyout.Items.Add(new MenuFlyoutItem { Text = ViewModel.UserMenuText, IsEnabled = false });
            flyout.Items.Add(new MenuFlyoutSeparator());
            flyout.Items.Add(new MenuFlyoutItem { Text = "Dodaj grę", Command = ViewModel.AddGameCommand });
            flyout.Items.Add(new MenuFlyoutItem { Text = "Moja biblioteka", Command = ViewModel.MyGamesCommand });
            flyout.Items.Add(new MenuFlyoutItem { Text = "Wyloguj się", Command = ViewModel.LogoutCommand });
        }
        else
        {
            flyout.Items.Add(new MenuFlyoutItem { Text = "Zaloguj się", Command = ViewModel.LoginCommand });
        }

        flyout.ShowAt((FrameworkElement)sender);
    }

}
