using RubbishRumble.Services;
using RubbishRumble.ViewModels;

namespace RubbishRumble.Views;

public partial class MainMenuPage : ContentPage
{
    public MainMenuPage()
    {
        InitializeComponent();
        BindingContext = new MainMenuViewModel();
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await SettingsService.Instance.PlayMusicAsync("bgmusic.mp3");
    }
}