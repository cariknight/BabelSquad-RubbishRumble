using RubbishRumble.ViewModels;
namespace RubbishRumble.Views;

public partial class GamePage : ContentPage
{
    private readonly GameViewModel _viewModel;

    public GamePage()
    {
        InitializeComponent();
        _viewModel = new GameViewModel();
        BindingContext = _viewModel;
        Appearing += OnAppearing;
    }

    private async void OnAppearing(object? sender, EventArgs e)
    {
        await _viewModel.InitializeAsync();
    }
}