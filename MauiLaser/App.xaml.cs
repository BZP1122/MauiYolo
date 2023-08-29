using MauiLaser.Views;


namespace MauiLaser
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Emgu.CV.Platform.Maui.MauiInvoke.Init();
            Routing.RegisterRoute("MainPage", typeof(MainPage));
            Routing.RegisterRoute("ImageInfoPage", typeof(ImageInfoPage));
            MainPage = new AppShell();
        }
    }
}
