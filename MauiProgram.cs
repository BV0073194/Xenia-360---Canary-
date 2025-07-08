using Microsoft.Extensions.Logging;

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

            // This line requires the Microsoft.Extensions.Logging.Debug NuGet package
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}