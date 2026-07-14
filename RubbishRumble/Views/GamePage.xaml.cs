using RubbishRumble.ViewModels;
namespace RubbishRumble.Views;

public partial class GamePage : ContentPage
{
    public GamePage()
    {
        InitializeComponent();
        BindingContext = new GameViewModel();
    }

    private async void OnExitButtonClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}