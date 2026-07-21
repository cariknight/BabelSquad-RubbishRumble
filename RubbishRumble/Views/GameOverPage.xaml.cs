using Microsoft.Maui.Controls;
using RubbishRumble.ViewModels;
using RubbishRumble.Services;

namespace RubbishRumble.Views;

public partial class GameOverPage : ContentPage, IQueryAttributable
{
    private readonly GameOverViewModel _viewModel = new();

	public GameOverPage()
	{
		InitializeComponent();
        BindingContext = _viewModel;

        Shell.SetBackButtonBehavior(this, new BackButtonBehavior
        {
            IsVisible = false,
            IsEnabled = false
        });
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("TotalScore", out object? scoreValue))
            _viewModel.TotalScore = ParseInt(scoreValue);

        if (query.TryGetValue("EarnedCoins", out object? coinsValue))
            _viewModel.EarnedCoins = ParseInt(coinsValue);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await SettingsService.Instance.PlaySfxAsync("gameover.mp3");
        RemoveGamePageFromStack();
        await _viewModel.SaveRewardsAsync();
        await _viewModel.LoadEcoTipAsync();
    }

    protected override bool OnBackButtonPressed()
    {
        return true;
    }

    private void RemoveGamePageFromStack()
    {
        Page? gamePage = Navigation.NavigationStack
            .FirstOrDefault(page => page is GamePage);

        if (gamePage != null)
            Navigation.RemovePage(gamePage);
    }

    private static int ParseInt(object value)
    {
        return value switch
        {
            int intValue => intValue,
            string stringValue when int.TryParse(stringValue, out int parsed) => parsed,
            _ => 0
        };
    }
}
