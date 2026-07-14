using System.Windows.Input;
using RubbishRumble.Services;

namespace RubbishRumble.ViewModels
{
    [QueryProperty(nameof(TotalScore), "TotalScore")]
    [QueryProperty(nameof(EarnedCoins), "EarnedCoins")]
    public class GameOverViewModel : BindableObject
    {
        private int _totalScore;
        private int _earnedCoins;

        public int TotalScore
        {
            get => _totalScore;
            set
            {
                _totalScore = value;
                OnPropertyChanged();
            }
        }

        public int EarnedCoins
        {
            get => _earnedCoins;
            set
            {
                _earnedCoins = value;
                OnPropertyChanged();
            }
        }

        public ICommand ExitCommand { get; }

        public GameOverViewModel()
        {
            ExitCommand = new Command(async () => await OnExitExecutedAsync());
            _ = SettingsService.Instance.PlaySfxAsync("sfxsound.mp3");
        }

        private static async Task OnExitExecutedAsync()
        {
            if (Shell.Current == null)
                return;

            await Shell.Current.GoToAsync("//MainMenuPage");
        }
    }
}
