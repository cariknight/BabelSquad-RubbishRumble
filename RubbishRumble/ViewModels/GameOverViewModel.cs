using System.Windows.Input;
using RubbishRumble.Helper;
using RubbishRumble.Services;

namespace RubbishRumble.ViewModels
{
    public class GameOverViewModel : BindableObject
    {
        private readonly DatabaseService _databaseService = new();
        private readonly APIService _apiService = new();
        private int _totalScore;
        private int _earnedCoins;
        private int _highestScore;
        private bool _isNewHighScore;
        private bool _rewardsSaved;
        private string _ecoTipTitle = string.Empty;
        private string _ecoTipText = "Loading eco fact...";

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
                OnPropertyChanged(nameof(CoinBonusText));
                OnPropertyChanged(nameof(ShowCoinBonus));
            }
        }

        public string HighScoreLabel => IsNewHighScore ? "NEW HIGH SCORE!" : "HIGHEST SCORE:";

        public string CoinBonusText => IsNewHighScore
            ? $"{Constants.HIGH_SCORE_COIN_MULTIPLIER:0.#}x high score bonus!"
            : string.Empty;

        public bool ShowCoinBonus => IsNewHighScore;

        public string EcoTipTitle
        {
            get => _ecoTipTitle;
            private set
            {
                _ecoTipTitle = value;
                OnPropertyChanged();
            }
        }

        public string EcoTipText
        {
            get => _ecoTipText;
            private set
            {
                _ecoTipText = value;
                OnPropertyChanged();
            }
        }

        public ICommand ExitCommand { get; }

        public GameOverViewModel()
        {
            ExitCommand = new Command(async () => await OnExitExecutedAsync());
        }

        public async Task SaveRewardsAsync()
        {
            if (_rewardsSaved)
                return;

            _rewardsSaved = true;

            try
            {
                int previousHighScore = (await _databaseService.GetPlayerAsync()).HighestScore;
                IsNewHighScore = TotalScore > 0 && TotalScore > previousHighScore;

                int coinsToAward = EconomyHelper.CalculateEarnedCoins(TotalScore, IsNewHighScore);
                EarnedCoins = coinsToAward;

                HighestScore = await _databaseService.AwardGameRewardsAsync(TotalScore, coinsToAward);
            }
            catch (Exception ex)
            {
                _rewardsSaved = false;
                System.Diagnostics.Debug.WriteLine($"Failed to save game rewards: {ex}");
            }
        }

        public async Task LoadEcoTipAsync()
        {
            try
            {
                (string title, string text) = await _apiService.GetRandomEcoTipAsync();
                EcoTipTitle = title;
                EcoTipText = text;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load eco tip: {ex}");
                EcoTipTitle = "Eco Fact";
                EcoTipText = "Keep sorting trash into the right bins to help the planet!";
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
