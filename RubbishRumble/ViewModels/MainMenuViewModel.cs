using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RubbishRumble.ViewModels
{
    public class MainMenuViewModel : BaseViewModel
    {
        public ICommand PlayCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand TutorialCommand { get; }
        public ICommand IndexCommand { get; }

        public MainMenuViewModel()
        {
            PlayCommand = new Command(OnPlayExecuted);
            SettingsCommand = new Command(OnSettingsExecuted);
            ExitCommand = new Command(OnSettingsExecuted);
            TutorialCommand = new Command(OnTutorialExecuted);
            IndexCommand = new Command(OnIndexExecuted);
        }

        private async void OnPlayExecuted()
        {
            await Shell.Current.GoToAsync("GamePage");
        }

        private async void OnSettingsExecuted()
        {
            await Application.Current.MainPage.DisplayAlert("Rubbish Rumble", "Working", "OK");
        }

        private async void OnTutorialExecuted()
        {
            await Application.Current.MainPage.DisplayAlert("Rubbish Rumble", "Working", "OK");
        }

        private async void OnIndexExecuted()
        {
            await Application.Current.MainPage.DisplayAlert("Rubbish Rumble", "Working", "OK");
        }
    }
}
