namespace RubbishRumble
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("TutorialPage", typeof(Views.TutorialPage));
        }
    }
}
