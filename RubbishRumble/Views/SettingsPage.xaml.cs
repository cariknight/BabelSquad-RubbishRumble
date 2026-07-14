using RubbishRumble.ViewModels;
using RubbishRumble.Services;
namespace RubbishRumble.Views;

public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
		InitializeComponent();

        BindingContext = new SettingsViewModel();
    }
    private async void OnExitButtonClicked(object sender, EventArgs e)
    {
        await SettingsService.Instance.PlaySfxAsync("sfxsound.mp3");
        await Shell.Current.GoToAsync("..");
    }
}