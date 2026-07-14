using RubbishRumble.Services;
using RubbishRumble.ViewModels;

namespace RubbishRumble.Views;

public partial class MainMenuPage : ContentPage
{
    public MainMenuPage()
    {
        InitializeComponent();
        BindingContext = new MainMenuViewModel();
        _ = SettingsService.Instance.InitializeMusicAsync();
    }
}