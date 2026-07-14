namespace RubbishRumble
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("GamePage", typeof(Views.GamePage));
            Routing.RegisterRoute("TutorialPage", typeof(Views.TutorialPage));
        }
    }
}
