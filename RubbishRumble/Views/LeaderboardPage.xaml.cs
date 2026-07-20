using RubbishRumble.Services;
using RubbishRumble.ViewModels;

namespace RubbishRumble.Views;

public partial class LeaderboardPage : ContentPage
{
	public LeaderboardPage()
	{
		InitializeComponent();

        BindingContext = new LeaderboardViewModel();
    }

    private async void OnExitButtonClicked(object sender, EventArgs e)
    {
        await SettingsService.Instance.PlaySfxAsync("sfxsound.mp3");
        await Shell.Current.GoToAsync("..");
    }
}