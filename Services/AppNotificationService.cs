using System.Collections.Specialized;
using System.Web;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppNotifications;

using SGSClient.Contracts.Services;
using SGSClient.ViewModels;
using WinUIEx.Messaging;

namespace SGSClient.Notifications;

public class AppNotificationService : IAppNotificationService
{
    private readonly INavigationService _navigationService;

    public AppNotificationService(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    ~AppNotificationService()
    {
        Unregister();
    }

    public void Initialize()
    {
        AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;

        AppNotificationManager.Default.Register();
    }

    public void OnNotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args)
    {
        string notificationName = args.Argument; // Pobierz nazwę powiadomienia z argumentów

        // TODO: Handle notification invocations when your app is already running.

        //// // Navigate to a specific page based on the notification arguments.
        //// if (ParseArguments(args.Argument)["action"] == "Settings")
        //// {
        ////    App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        ////    {
        ////        _navigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
        ////    });
        //// }

        //App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        //{
        //    App.MainWindow.ShowMessageDialogAsync("TODO: Handle notification invocations when your app is already running.", "Notification Invoked");

        //    App.MainWindow.BringToFront();
        //});

        App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
        {
            switch (notificationName)
            {
                case "SGSClientWelcomeNotificationPayload":
                    await App.MainWindow.ShowMessageDialogAsync("Aktualizowanie systemu z poziomu Sklepu Play\r\nAndroid 12 ma też przynieść zmiany w sposobie, w jaki aktualizuje nasze smartfony. To rozwinięcie idei Project Mainline, która ma uprościć całą procedurę. W zamyśle system mielibyśmy aktualizować bezpośrednio z poziomu Sklepu Play – zupełnie tak samo, jak wszystkie inne aplikacje na naszym telefonie.\r\n\r\nTeoretycznie mogłoby to oznaczać aktualizacje systemu prosto od Google, z pominięciem producentów telefonów. Wszyscy wiemy, że przełożyłoby się to na znacznie szybsze wypuszczanie poprawek zabezpieczeń.", "SGSClient 2.0");
                    break;
                default:
                    //await App.MainWindow.ShowMessageDialogAsync($"Kliknięto inne powiadomienie o nazwie: {notificationName}", "Notification Invoked");
                    break;
            }
        });
    }
    public bool Show(string payload)
    {
        var appNotification = new AppNotification(payload);

        AppNotificationManager.Default.Show(appNotification);

        return appNotification.Id != 0;
    }

    public NameValueCollection ParseArguments(string arguments)
    {
        return HttpUtility.ParseQueryString(arguments);
    }

    public void Unregister()
    {
        AppNotificationManager.Default.Unregister();
    }
}
