using RubbishRumble.ViewModels;
namespace RubbishRumble.Views;

public partial class GamePage : ContentPage
{
    public GamePage()
    {
        InitializeComponent();
        BindingContext = new GameViewModel();
    }
}