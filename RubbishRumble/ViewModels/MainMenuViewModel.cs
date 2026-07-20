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
        public ICommand LeaderboardCommand { get; }
        public MainMenuViewModel()
        {
            PlayCommand = new Command(OnPlayExecuted);
            SettingsCommand = new Command(OnSettingsExecuted);
            StoreCommand = new Command(OnStoreExecuted);
            TutorialCommand = new Command(OnTutorialExecuted);
            LeaderboardCommand = new Command(OnLeaderboardExecuted);
        }

        private async Task NavigateAsync(string route)
        {
            if (Shell.Current == null)
                return;

            await SettingsService.Instance.PlaySfxAsync("sfxsound.mp3");
            await Shell.Current.GoToAsync(route);
        }

        private async void OnPlayExecuted()
        {
            await NavigateAsync("GamePage");
        }

        private async void OnSettingsExecuted()
        {
            await NavigateAsync("SettingsPage");
        }

        private async void OnTutorialExecuted()
        {
            await NavigateAsync("TutorialPage");
        }

        private async void OnStoreExecuted()
        {
            await NavigateAsync("StorePage");
        }
        private async void OnLeaderboardExecuted()
        {
            await NavigateAsync("LeaderboardPage");
        }
    }
}
