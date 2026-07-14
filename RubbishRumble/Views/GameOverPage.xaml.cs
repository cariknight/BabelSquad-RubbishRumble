using RubbishRumble.ViewModels;

namespace RubbishRumble.Views;

public partial class GameOverPage : ContentPage
{
	public GameOverPage()
	{
		InitializeComponent();
        BindingContext = new GameOverViewModel();
    }
}