using RubbishRumble.ViewModels;

namespace RubbishRumble.Views;

public partial class LeaderboardPage : ContentPage
{
	public LeaderboardPage()
	{
		InitializeComponent();

        BindingContext = new LeaderboardViewModel();
    }
}