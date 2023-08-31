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
                    await App.MainWindow.ShowMessageDialogAsync("Dziękujemy za zainstalowanie najnowszej wersji SGSClienta! \r\nChangeLog: Wersja 2.1.0.0:" +
                        "\r\n\r\n• Nowy Mechanizm Aktualizacji: Wprowadzono nowy, zoptymalizowany mechanizm aktualizacji, który pozwala użytkownikom szybko i sprawnie aktualizować oprogramowanie do najnowszej wersji. To umożliwia łatwe dostęp do nowych funkcji i poprawek." +
                        "\r\n\r\n• Zamiana WebClient na HttpClient: Zaktualizowano architekturę klienta sieciowego, przechodząc z WebClient na HttpClient. Ta zmiana przynosi znaczącą poprawę procesu pobierania wszystkich gier, co skutkuje szybszym i bardziej wydajnym sposobem pobierania treści." +
                        "\r\n\r\n• Zaaktualizowano Elementy Wyświetlane na Stronie z Daną Grą: Wprowadzono usprawnienia w wyświetlaniu zawartości związanej z konkretnymi grami na stronie głównej. Teraz prezentacja treści jest bardziej atrakcyjna i intuicyjna." +
                        "\r\n\r\n• Przycisk Zgłoś Błąd: Dodano nowy przycisk \"Zgłoś Błąd\" na stronie głównej aplikacji. Teraz użytkownicy mogą łatwo zgłaszać napotkane błędy i problemy, co przyczynia się do ciągłego doskonalenia jakości oprogramowania." + 
                        "\r\n\r\n• Optymalizacja Rozmiaru Okna: Przeprowadzono optymalizację rozmiaru okna aplikacji, co przekłada się na lepsze wykorzystanie dostępnego miejsca na ekranie. Interfejs użytkownika jest teraz bardziej responsywny i komfortowy." + 
                        "\r\n\r\n• Poprawki Wizualne dla Jasnego Motywu: Naprawiono problem z niepoprawnym wyświetlaniem elementów górnego paska okna w jasnym motywie. Teraz interfejs prezentuje się spójnie i estetycznie, niezależnie od wybranego motywu."
                        , "SGSClient 2.1 - Witamy");
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
