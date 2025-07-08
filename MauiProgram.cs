using Microsoft.Extensions.Logging;
using Xenia_360____Canary_.Services;
using Xenia_360____Canary_.ViewModels;
using Xenia_360____Canary_.Views;

namespace Xenia_360____Canary_
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // --- Dependency Injection Setup ---

            // Register Services as Singletons (one shared instance for the whole app)
            builder.Services.AddSingleton<SettingsService>();
            builder.Services.AddSingleton<DownloadManagerService>();
            builder.Services.AddSingleton<XeniaLauncherService>();
            builder.Services.AddSingleton<ToolManagerService>();


            // Register ViewModels as Transient (a new one is created every time it's requested)
            builder.Services.AddTransient<GameLibraryViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();
            builder.Services.AddTransient<DownloadQueueViewModel>();


            // Register Pages as Transient
            builder.Services.AddTransient<GameLibraryPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<DownloadQueuePage>();
            builder.Services.AddTransient<XeniaStorePage>();
            // --- End of Dependency Injection Setup ---

            return builder.Build();
        }
    }
}