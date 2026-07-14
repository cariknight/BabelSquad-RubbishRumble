using System.Windows.Input;
using RubbishRumble.Services;

namespace RubbishRumble.ViewModels
{
    public class GameOverViewModel : BindableObject
    {
        private readonly DatabaseService _databaseService = new();
        private int _totalScore;
        private int _earnedCoins;
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
                await _databaseService.AwardGameRewardsAsync(TotalScore, EarnedCoins);
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
