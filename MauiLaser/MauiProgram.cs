using Camera.MAUI;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using MauiLaser.Core.Interfaces;
using MauiLaser.Core.Models;
using MauiLaser.ViewModels;
using MauiLaser.Views;

namespace MauiLaser
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkitCore()
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitMediaElement()
                .UseMauiCameraView()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.Services.AddScoped<MainPage>();
            builder.Services.AddScoped<MainPageViewModel>();
            builder.Services.AddScoped<ITakePhoto, TakePhoto>();
            return builder.Build();
        }
    }
}
