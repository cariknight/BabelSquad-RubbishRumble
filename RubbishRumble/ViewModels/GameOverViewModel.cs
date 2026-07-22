using System.Collections.ObjectModel;
using System.Windows.Input;
using RubbishRumble.Helper;
using RubbishRumble.Models;
using RubbishRumble.Services;

namespace RubbishRumble.ViewModels
{
    public class GameOverViewModel : BindableObject
    {
        private readonly DatabaseService _databaseService = new();
        private readonly PowerUpService _powerUpService = new();
        private readonly InventoryService _inventoryService;
        private readonly APIService _apiService = new();
        private readonly LeaderboardService _leaderboardService = new();
        private int _totalScore;
        private int _earnedCoins;
        private int _highestScore;
        private int _currentRevivePower;
        private bool _isNewHighScore;
        private bool _rewardsSaved;
        private bool _leaderboardSubmitted;
        private bool _isLeaderboardPopupVisible;
        private string _playerNameText = string.Empty;
        private AvatarOption? _selectedAvatar;
        private string _submitErrorMessage = string.Empty;
        private string _ecoTipTitle = string.Empty;
        private string _ecoTipText = "Loading eco fact...";

        public int CurrentRevivePower
        {
            get => _currentRevivePower;
            private set
            {
                _currentRevivePower = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanUseRevive));
                OnPropertyChanged(nameof(ReviveButtonOpacity));
            }
        }

        public bool CanUseRevive =>
            CurrentRevivePower > 0 && GameViewModel.ActiveInstance != null;

        public double ReviveButtonOpacity => CanUseRevive ? 1.0 : 0.4;

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

        public bool IsLeaderboardPopupVisible
        {
            get => _isLeaderboardPopupVisible;
            private set
            {
                _isLeaderboardPopupVisible = value;
                OnPropertyChanged();
            }
        }

        public string PlayerNameText
        {
            get => _playerNameText;
            set
            {
                _playerNameText = value;
                OnPropertyChanged();
            }
        }

        public AvatarOption? SelectedAvatar
        {
            get => _selectedAvatar;
            set
            {
                _selectedAvatar = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedAvatarImage));
            }
        }

        public string SelectedAvatarImage => SelectedAvatar?.ImagePath ?? string.Empty;

        public string SubmitErrorMessage
        {
            get => _submitErrorMessage;
            private set
            {
                _submitErrorMessage = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<AvatarOption> Avatars { get; } = new();

        public ICommand ExitCommand { get; }
        public ICommand UseReviveCommand { get; }
        public ICommand SubmitLeaderboardCommand { get; }

        public GameOverViewModel()
        {
            _inventoryService = new InventoryService(_databaseService, _powerUpService);
            ExitCommand = new Command(async () => await OnExitExecutedAsync());
            UseReviveCommand = new Command(async () => await OnReviveExecutedAsync());
            SubmitLeaderboardCommand = new Command(async () => await OnSubmitLeaderboardAsync());
        }

        public async Task LoadReviveCountAsync()
        {
            await _powerUpService.InitializeAsync();
            Player player = await _databaseService.GetPlayerAsync();
            await _inventoryService.EnsureBeginnerRevivesAsync(player);
            CurrentRevivePower = await _inventoryService.GetPowerUpCountAsync("Revive");
            OnPropertyChanged(nameof(CanUseRevive));
            OnPropertyChanged(nameof(ReviveButtonOpacity));
        }

        public async Task LoadRewardsPreviewAsync()
        {
            try
            {
                Player player = await _databaseService.GetPlayerAsync();
                int previousHighScore = player.HighestScore;
                IsNewHighScore = TotalScore > 0 && TotalScore > previousHighScore;
                EarnedCoins = EconomyHelper.CalculateEarnedCoins(TotalScore, IsNewHighScore);
                HighestScore = Math.Max(previousHighScore, TotalScore);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load reward preview: {ex}");
            }
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

        public void InitializeLeaderboardPopup()
        {
            Avatars.Clear();

            foreach (AvatarOption avatar in _leaderboardService.GetAvailableAvatars())
                Avatars.Add(avatar);

            SelectedAvatar = Avatars.FirstOrDefault();
            PlayerNameText = string.Empty;
            SubmitErrorMessage = string.Empty;
            IsLeaderboardPopupVisible = IsNewHighScore;
        }

        private async Task OnSubmitLeaderboardAsync()
        {
            SubmitErrorMessage = string.Empty;

            if (SelectedAvatar == null)
                return;

            await SaveRewardsAsync();

            LeaderboardSubmitResult result = await _leaderboardService.SubmitLeaderboardEntryAsync(
                PlayerNameText,
                SelectedAvatar.Id,
                TotalScore);

            if (!result.Success)
            {
                SubmitErrorMessage = result.ErrorMessage;
                return;
            }

            _leaderboardSubmitted = true;
            IsLeaderboardPopupVisible = false;
            await SettingsService.Instance.PlaySfxAsync("sfxsound.mp3");
        }

        private async Task OnReviveExecutedAsync()
        {
            GameViewModel? activeGame = GameViewModel.ActiveInstance;
            if (activeGame == null || !CanUseRevive)
                return;

            if (!await activeGame.TryReviveAndResumeAsync())
                return;

            CurrentRevivePower = await _inventoryService.GetPowerUpCountAsync("Revive");

            if (Shell.Current == null)
                return;

            await Shell.Current.GoToAsync("..");
        }

        private async Task OnExitExecutedAsync()
        {
            await SaveRewardsAsync();

            if (IsNewHighScore && !_leaderboardSubmitted)
                await _databaseService.ClearPendingLeaderboardScoreAsync();

            GameViewModel.ActiveInstance?.PrepareToQuit();

            if (Shell.Current == null)
                return;

            await Shell.Current.GoToAsync("//MainMenuPage");
        }
    }
}
