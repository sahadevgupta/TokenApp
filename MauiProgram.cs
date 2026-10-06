using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using TokenApp.Controls;
using TokenApp.Services;
using TokenApp.Services.Data;
using TokenApp.Services.Database;
using TokenApp.ViewModels;
using TokenApp.Views;

namespace TokenApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMauiApp<App>()
                .UseSentry(options => {
                    options.Dsn = "https://d7a70ae5f15d9c1567ccbe6ba0881147@o4510259879673856.ingest.de.sentry.io/4512204335743056";
#if DEBUG
                    options.Debug = true;
#endif
                    options.TracesSampleRate = 1.0;
                    // Enable logs to be sent to Sentry
                    options.EnableLogs = true;

                    // Other Sentry options can be set here.
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("fa-solid-900.ttf", "MyFont");

                    fonts.AddFont("Geist-Regular.ttf", "GeistRegular");
                    fonts.AddFont("Geist-Medium.ttf", "GeistMedium");
                    fonts.AddFont("SpaceGrotesk-SemiBold.ttf", "SpaceGroteskSemibold");
                    fonts.AddFont("SpaceGrotesk-Bold.ttf", "SpaceGroteskBold");
                })
                .ConfigureMauiHandlers(handlers =>
                {
#if ANDROID
                    handlers.AddHandler<BorderlessEntry, TokenApp.Platforms.Android.Handlers.PlainEntryHandler>();
#endif
                })
                // Register Services
                .RegisterAppServices();

            EntryHandler.Mapper.AppendToMapping(nameof(BorderlessEntry), (handler, view) =>
            {
                if (view is BorderlessEntry control)
                {
#if IOS
                    handler.PlatformView.BackgroundColor = Colors.Transparent.ToPlatform();
                    handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
                    handler.PlatformView.ClipsToBounds = true;
                    handler.PlatformView.Layer.BorderWidth = 0;
                    handler.PlatformView.Layer.BorderColor = UIKit.UIColor.Clear.CGColor;
#elif ANDROID
                    handler.PlatformView.InputType = Android.Text.InputTypes.TextVariationShortMessage;
                    handler.PlatformView.SetBackgroundColor(Colors.Transparent.ToPlatform());
                    handler.PlatformView.Background = null; 

#endif
                }
            });

#if DEBUG
            builder.Logging.AddDebug();
#endif

#if ANDROID
            // Register Android-specific printer service implementation
            builder.Services.AddSingleton<TokenApp.Interfaces.IPrinterService, TokenApp.Platforms.Android.Printer.AndroidPrinterService>();
#endif

            return builder.Build();
        }

        private static MauiAppBuilder RegisterAppServices(this MauiAppBuilder builder)
        {
            // Register Database
            builder.Services.AddSingleton(AppDbContext.Instance);

            // Register Data Services
            builder.Services.AddSingleton<ITokenMapper, TokenMapper>();
            builder.Services.AddSingleton<ITokenRepository, TokenRepository>();

            // Register ViewModels
            builder.Services.AddSingleton<TokenHistoryViewModel>();

            // Register Views
            builder.Services.AddSingleton<TokenHistoryPage>();

            // Initialize database on startup
            builder.Services.AddSingleton<LazyDbInitializer>();

            return builder;
        }
    }
}
