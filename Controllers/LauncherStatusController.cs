namespace SGSClient.Controllers;

enum LauncherStatus
{
    pageLauched, //strona z grą została odpalona
    ready, //gra gotowa do uruchomienia
    readyNoGame, //brak gry
    failed, //gra błędnie sciagnieta
    downloadingGame, //pobieranie gry
    downloadingUpdate //pobieranie update
}