using RubbishRumble.ViewModels;
using RubbishRumble.Services;

namespace RubbishRumble.Views;

public partial class GamePage : ContentPage
{
    public GamePage()
    {
        InitializeComponent();
        BindingContext = new GameViewModel();
    }

    private async void OnPauseButtonClicked(object sender, EventArgs e)
    {
        await SettingsService.Instance.PlaySfxAsync("sfxsound.mp3");
    }
}