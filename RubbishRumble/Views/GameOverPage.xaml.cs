using RubbishRumble.ViewModels;

namespace RubbishRumble.Views;

public partial class GameOverPage : ContentPage, IQueryAttributable
{
    private readonly GameOverViewModel _viewModel = new();

	public GameOverPage()
	{
		InitializeComponent();
        BindingContext = _viewModel;
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
        await _viewModel.SaveRewardsAsync();
        await _viewModel.LoadEcoTipAsync();
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
