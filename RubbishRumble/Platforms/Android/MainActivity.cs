using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace RubbishRumble
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HideStatusBar();
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);
            if (hasFocus)
            {
                HideStatusBar();
            }
        }

        private void HideStatusBar()
        {
            if (Window?.DecorView is null)
            {
                return;
            }

            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                Window.SetDecorFitsSystemWindows(false);
                var controller = Window.InsetsController;
                if (controller is not null)
                {
                    controller.Hide(WindowInsets.Type.StatusBars());
                    controller.SystemBarsBehavior = (int)WindowInsetsControllerBehavior.ShowTransientBarsBySwipe;
                }
            }
            else
            {
#pragma warning disable CA1422, CS0618
                Window.DecorView.SystemUiVisibility = (StatusBarVisibility)(
                    (int)SystemUiFlags.ImmersiveSticky
                    | (int)SystemUiFlags.Fullscreen
                    | (int)SystemUiFlags.LayoutStable
                    | (int)SystemUiFlags.LayoutFullscreen);
#pragma warning restore CA1422, CS0618
            }
        }
    }
}
