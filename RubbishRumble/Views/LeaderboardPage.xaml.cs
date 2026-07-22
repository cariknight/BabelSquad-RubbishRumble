using RubbishRumble.Services;
using RubbishRumble.ViewModels;

namespace RubbishRumble.Views;

public partial class LeaderboardPage : ContentPage
{
    private readonly LeaderboardViewModel _viewModel = new();

	public LeaderboardPage()
	{
		InitializeComponent();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }

    private async void OnExitButtonClicked(object sender, EventArgs e)
    {
        await SettingsService.Instance.PlaySfxAsync("sfxsound.mp3");
        await Shell.Current.GoToAsync("..");
    }
}