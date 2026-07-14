using RubbishRumble.Services;
using RubbishRumble.ViewModels;

namespace RubbishRumble.Views;

public partial class StorePage : ContentPage
{
    private readonly StoreViewModel _viewModel;

    public StorePage()
    {
        InitializeComponent();
        _viewModel = new StoreViewModel();
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
