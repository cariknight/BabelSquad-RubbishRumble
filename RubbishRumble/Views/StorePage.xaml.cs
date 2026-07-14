namespace RubbishRumble.Views;

public partial class StorePage : ContentPage
{
	public StorePage()
	{
		InitializeComponent();
	}

    private async void OnExitButtonClicked(object sender, EventArgs e)
    {
        // This pops the store page off the stack and returns you to the Main Menu
        await Shell.Current.GoToAsync("..");
    }
}