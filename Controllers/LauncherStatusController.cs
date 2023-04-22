namespace SGSClient.Controllers;

enum LauncherStatus
{
    ready, //gra gotowa do uruchomienia
    failed, //gra błędnie sciagnieta
    downloadingGame, //pobieranie gry
    downloadingUpdate //pobieranie update
}