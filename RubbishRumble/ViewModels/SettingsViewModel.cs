using System.Windows.Input;
using RubbishRumble.Services;

namespace RubbishRumble.ViewModels
{
    public class SettingsViewModel : BindableObject
    {
        public bool IsMusicMuted
        {
            get => SettingsService.Instance.IsMusicMuted;
            set
            {
                if (SettingsService.Instance.IsMusicMuted == value)
                    return;

                SettingsService.Instance.SetMusicMuted(value);
                OnPropertyChanged();
            }
        }

        public bool IsSfxMuted
        {
            get => SettingsService.Instance.IsSfxMuted;
            set
            {
                if (SettingsService.Instance.IsSfxMuted == value)
                    return;

                SettingsService.Instance.SetSfxMuted(value);
                OnPropertyChanged();
            }
        }
        public ICommand ToggleMusicCommand { get; }
        public ICommand ToggleSfxCommand { get; }

        public SettingsViewModel()
        {
            ToggleMusicCommand = new Command(OnToggleMusic);
            ToggleSfxCommand = new Command(OnToggleSfx);
        }

        private void OnToggleMusic()
        {
            IsMusicMuted = !IsMusicMuted;
        }

        private async void OnToggleSfx()
        {
            IsSfxMuted = !IsSfxMuted;
            if (!IsSfxMuted)
            {
                await SettingsService.Instance.PlaySfxAsync("sfxsound.mp3");
            }
        }
    }
}
