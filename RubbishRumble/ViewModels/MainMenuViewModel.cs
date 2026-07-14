using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using RubbishRumble.Services;

namespace RubbishRumble.ViewModels
{
    public class MainMenuViewModel : BaseViewModel
    {
        public ICommand PlayCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand StoreCommand { get; }
        public ICommand TutorialCommand { get; }

        public MainMenuViewModel()
        {
            PlayCommand = new Command(OnPlayExecuted);
            SettingsCommand = new Command(OnSettingsExecuted);
            StoreCommand = new Command(OnStoreExecuted);
            TutorialCommand = new Command(OnTutorialExecuted);
        }

        private async void OnPlayExecuted()
        {
            await SettingsService.Instance.PlaySfxAsync("sfxsound.mp3");
            await Shell.Current.GoToAsync("GamePage");
        }

        private async void OnSettingsExecuted()
        {
            await SettingsService.Instance.PlaySfxAsync("sfxsound.mp3");
            await Shell.Current.GoToAsync("SettingsPage");
        }

        private async void OnTutorialExecuted()
        {
            await SettingsService.Instance.PlaySfxAsync("sfxsound.mp3");
            await Shell.Current.GoToAsync("TutorialPage");
        }

        private async void OnStoreExecuted()
        {
            await SettingsService.Instance.PlaySfxAsync("sfxsound.mp3");
            await Shell.Current.GoToAsync("StorePage");
        }
    }
}
