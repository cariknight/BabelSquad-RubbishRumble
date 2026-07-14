using RubbishRumble.Services;
namespace RubbishRumble.Views;

public partial class StorePage : ContentPage
{
	public StorePage()
	{
		InitializeComponent();
	}

    private async void OnExitButtonClicked(object sender, EventArgs e)
    {
        await SettingsService.Instance.PlaySfxAsync("sfxsound.mp3");
        await Shell.Current.GoToAsync("..");
    }
}