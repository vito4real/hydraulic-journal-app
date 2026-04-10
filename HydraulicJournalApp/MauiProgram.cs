using HydraulicJournalApp.Services;
using Microsoft.Extensions.Logging;

namespace HydraulicJournalApp;

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

        var dbPath = Path.Combine(AppContext.BaseDirectory, "HydraulicJournal.db");
        builder.Services.AddSingleton(new DatabaseService(dbPath));
        builder.Services.AddSingleton<AccessGuardService>();

        return builder.Build();
    }
}