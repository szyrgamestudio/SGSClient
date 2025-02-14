using System.Collections.Specialized;
using System.Web;
using Microsoft.Windows.AppNotifications;

using SGSClient.Contracts.Services;

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
                    await App.MainWindow.ShowMessageDialogAsync("Dziękujemy za zainstalowanie najnowszej wersji SGSClienta! \r\nChangeLog: Wersja 2.5.0.0:" +
                        "\r\n\r\n• System kont: xxx" +
                        "\r\n\r\n• Można dodać gre: xxx" +
                        "\r\n\r\n• Kolejna jakaś rzecz: xxx"
                        , "SGSClient 2.5 - Witamy");
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
