using RubbishRumble.Services;

namespace RubbishRumble
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            SettingsService.Instance.PauseForAppInactive();
        }

        protected override void OnResume()
        {
            base.OnResume();
            SettingsService.Instance.ResumeFromAppActive();
        }
    }
}