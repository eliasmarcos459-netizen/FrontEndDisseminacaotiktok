using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Microsoft.Extensions.DependencyInjection;
using SMAD;

namespace FrontEndDisseminacaoRedesSociais
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

           
            builder.Services.AddMudServices();
            builder.Services.AddSingleton(TimeProvider.System);
            builder.Services.AddHttpClient<FacebookService>();
            builder.Services.AddHttpClient<TikTokService>();
            builder.Services.AddHttpClient<InstagramService>();
            builder.Services.AddHttpClient<TwitterService>();
            builder.Services.AddSingleton<TikTokToken>();
            builder.Services.AddScoped<PostZenService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();

#endif
            return builder.Build();
        }
    }
}