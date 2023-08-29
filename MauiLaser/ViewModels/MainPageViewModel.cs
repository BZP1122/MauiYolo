using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MauiLaser.Core.Interfaces;
using Microsoft.CodeAnalysis.Completion;

namespace MauiLaser.ViewModels
{
    record SimpleMessage(string content);

    public partial class MainPageViewModel : ObservableObject
    {
        [ObservableProperty]
        bool isActive = true;

        ITakePhoto photoService;
        IMessenger message = WeakReferenceMessenger.Default;

        public MainPageViewModel(ITakePhoto takePhoto)
        {
            this.photoService = takePhoto;
            message.Register<SimpleMessage>(
                this,
                (_, m) =>
                {
                    Console.WriteLine(m.content);
                }
            );
            message.Send(this);
        }

        [RelayCommand(CanExecute = nameof(IsActive))]
        private async Task Navigate(object pageName)
        {
            try
            {
                var data = new SimpleMessage("11");
                SimpleMessage data1 = data with { };
                var data2 = new SimpleMessage("11");
                bool value = data.Equals(data2);
                message.Send(data);
                await Shell.Current.GoToAsync(pageName?.ToString() + $"?name = {123}");
                //await photoService.TakePhotoAsyn();
                //await photoService.PickPhotoAsync();
            }
            catch (Exception ex) { }
        }
    }
}
