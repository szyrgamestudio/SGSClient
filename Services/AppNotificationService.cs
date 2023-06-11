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
                    await App.MainWindow.ShowMessageDialogAsync("Dziękujemy za zainstalowanie wersji 2.0 SGSClienta! \nJesteśmy bardzo wdzięczni za zaufanie, jakim obdarzyłeś naszą aplikację, i jesteśmy pewni, że wersja 2.0 przyniesie Ci jeszcze więcej satysfakcji i korzyści. Poniżej przedstawiamy kilka przykładowych zmian, które wprowadziliśmy w tej nowej wersji:" +
                        "\r\n\r\n• Nowy interfejs użytkownika: Zmieniliśmy wygląd i układ interfejsu, aby zapewnić bardziej intuicyjne i przyjemne doświadczenie użytkowania. Teraz nasz program stylistycznie przypomina natywną aplikację systemów Windows 11 oraz 10. Dzięki temu będziesz miał poczucie znajomości i spójności z innymi aplikacjami na Twoim komputerze. Łatwo znajdziesz potrzebne funkcje i narzędzia, a jednocześnie cieszyć się nowoczesnym wyglądem programu." +
                        "\r\n\r\n• Nowe funkcje: Dodaliśmy kilka nowych funkcji, które pozwolą Ci jeszcze bardziej wykorzystać nasz program. Od teraz możesz bezpośrednio w SGSCliencie wypełnić formularz, który umożliwi tobie złożenie wniosku o dodanie własnej gry do klienta." +
                        "\r\n\r\n• Aktualizacje przez Microsoft Store: Od teraz SGSClient jest dostępny w Microsoft Store, co przynosi wiele korzyści. Dzięki temu możesz cieszyć się wygodnym procesem aktualizacji, ponieważ teraz wszystkie aktualizacje programu będą odbywały się automatycznie przez Microsoft Store. Nie będziesz już musiał ręcznie sprawdzać i pobierać aktualizacji. To oznacza, że zawsze będziesz mieć dostęp do najnowszych funkcji i poprawek, gwarantując bezpieczeństwo i optymalne działanie programu." +
                        "\r\n\r\nTo tylko kilka przykładów zmian, które wprowadziliśmy w wersji 2.0. Wierzymy, że te ulepszenia uczynią Twoje doświadczenie z naszym klientem jeszcze bardziej satysfakcjonującym. Jeszcze raz dziękujemy za aktualizację i życzymy owocnej pracy z nową wersją SGSClienta!", "SGSClient 2.0 - Witamy");
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
