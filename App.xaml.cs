using System.Diagnostics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;

using SGSClient.Activation;
using SGSClient.Contracts.Services;
using SGSClient.Core;
using SGSClient.Core.Authorization;
using SGSClient.Core.Contracts.Services;
using SGSClient.Core.Interface;
using SGSClient.Core.Services;
using SGSClient.Core.Utilities;
using SGSClient.Core.Utilities.AppInfoUtility.Interfaces;
using SGSClient.Core.Utilities.AppInfoUtility.Models;
using SGSClient.Core.Utilities.LogUtility;
using SGSClient.Models;
using SGSClient.Notifications;
using SGSClient.Services;
using SGSClient.ViewModels;
using SGSClient.Views;

namespace SGSClient;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();
    public static UIElement? AppTitlebar { get; set; }
    private SimpleSplashScreen fss { get; set; } //https://github.com/dotMorten/WinUIEx/blob/main/docs/concepts/Splashscreen.md/
    public App()
    {
        fss = SimpleSplashScreen.ShowDefaultSplashScreen();
        InitializeComponent();
        //MainWindow.CenterOnScreen();
        Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", "--lang=pl-PL");

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            services.AddSingleton<IAppUser, AppUser>();
            services.AddSingleton<IAppInfo, AppInfo>();

            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers
            services.AddTransient<IActivationHandler, AppNotificationActivationHandler>();

            // Services
            services.AddSingleton<IAppNotificationService, AppNotificationService>();
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Core Services
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddSingleton<PasswordHasher, PasswordHasher>();

            // Views and ViewModels
            services.AddTransient<ProfileViewModel>();
            services.AddTransient<ProfilePage>();
            services.AddTransient<SettingsUserViewModel>();
            services.AddTransient<SettingsUserPage>();
            services.AddTransient<EditGameViewModel>();
            services.AddTransient<EditGamePage>();
            services.AddTransient<MyGamesViewModel>();
            services.AddTransient<MyGamesPage>();
            services.AddTransient<GameBaseViewModel>();
            services.AddTransient<GameBasePage>();
            services.AddTransient<UploadGameViewModel>();
            services.AddTransient<UploadGamePage>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SettingsPage>();
            services.AddTransient<GamesViewModel>();
            services.AddTransient<GamesPage>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<HomePage>();
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();

            // Configuration
            services.Configure<ConnectionStrings>(context.Configuration.GetSection(nameof(ConnectionStrings)));
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
            services.Configure<AzureADSettings>(context.Configuration.GetSection(nameof(AzureADSettings)));
            services.Configure<DiscordSettings>(context.Configuration.GetSection(nameof(DiscordSettings)));
        }).
        Build();

        App.GetService<IAppNotificationService>().Initialize();
        ServiceFactory.SetServiceProvider(Host.Services);


        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // Log the exception details (message, stack trace, etc.)
        LogException(e.Exception);

        // Mark the exception as handled to prevent the application from crashing
        e.Handled = true;
    }

    private void LogException(Exception ex)
    {
        _ = Log.ErrorAsync("Unhandled exception", ex); 
        // Simple logging: Log exception details to a file (or use a logging library)
        string logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "error_log.txt");

        try
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, append: true))
            {
                writer.WriteLine("====================================");
                writer.WriteLine($"Date: {DateTime.Now}");
                writer.WriteLine($"Message: {ex.Message}");
                writer.WriteLine($"StackTrace: {ex.StackTrace}");
                writer.WriteLine("====================================");
            }
        }
        catch (Exception loggingEx)
        {
            // If logging fails, we can ignore it for now
            Debug.WriteLine($"Failed to log exception: {loggingEx.Message}");
        }
    }
    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);
        fss?.Hide();
        fss = null;
        DiscordManager.Initialize();
        await App.GetService<IActivationService>().ActivateAsync(args);
    }
}
