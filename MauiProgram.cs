using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Coursework.Services;
using Coursework.Data;
using System.IO;

namespace Coursework
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
                });

            builder.Services.AddMauiBlazorWebView();

            // Register DbContext
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                var databasePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "journal.db"
                );
                options.UseSqlite($"Data Source={databasePath}");
            });

            // Register Database Service as Scoped (to work with DbContext)
            builder.Services.AddScoped<DatabaseService>();
            builder.Services.AddScoped<AuthenticationService>();
            builder.Services.AddScoped<ThemeService>();
            builder.Services.AddScoped<PdfExportService>();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
