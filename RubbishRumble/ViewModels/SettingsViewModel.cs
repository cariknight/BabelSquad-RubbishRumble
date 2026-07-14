using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls; 
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
                if (SettingsService.Instance.IsMusicMuted != value)
                {
                    SettingsService.Instance.ToggleMusic();
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSfxMuted
        {
            get => SettingsService.Instance.IsSfxMuted;
            set
            {
                if (SettingsService.Instance.IsSfxMuted != value)
                {
                    SettingsService.Instance.ToggleSfx();
                    OnPropertyChanged();
                }
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
