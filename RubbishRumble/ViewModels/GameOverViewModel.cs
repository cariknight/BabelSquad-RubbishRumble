using System.Windows.Input;
using RubbishRumble.Services;

namespace RubbishRumble.ViewModels
{
    public class GameOverViewModel : BindableObject
    {
        private readonly DatabaseService _databaseService = new();
        private int _totalScore;
        private int _earnedCoins;
        private int _highestScore;
        private bool _isNewHighScore;
        private bool _rewardsSaved;

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

        public int HighestScore
        {
            get => _highestScore;
            private set
            {
                _highestScore = value;
                OnPropertyChanged();
            }
        }

        public bool IsNewHighScore
        {
            get => _isNewHighScore;
            private set
            {
                _isNewHighScore = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HighScoreLabel));
            }
        }

        public string HighScoreLabel => IsNewHighScore ? "NEW HIGH SCORE!" : "HIGHEST SCORE:";

        public ICommand ExitCommand { get; }

        public GameOverViewModel()
        {
            ExitCommand = new Command(async () => await OnExitExecutedAsync());
            _ = SettingsService.Instance.PlaySfxAsync("sfxsound.mp3");
        }

        public async Task SaveRewardsAsync()
        {
            if (_rewardsSaved)
                return;

            _rewardsSaved = true;

            try
            {
                int previousHighScore = (await _databaseService.GetPlayerAsync()).HighestScore;
                HighestScore = await _databaseService.AwardGameRewardsAsync(TotalScore, EarnedCoins);
                IsNewHighScore = TotalScore > 0 && TotalScore >= HighestScore && TotalScore > previousHighScore;
            }
            catch (Exception ex)
            {
                _rewardsSaved = false;
                System.Diagnostics.Debug.WriteLine($"Failed to save game rewards: {ex}");
            }
        }

        private static async Task OnExitExecutedAsync()
        {
            if (Shell.Current == null)
                return;

            await Shell.Current.GoToAsync("//MainMenuPage");
        }
    }
}
