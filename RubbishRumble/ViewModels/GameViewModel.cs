using System.Windows.Input;
using RubbishRumble.Models;
using RubbishRumble.Services;

namespace RubbishRumble.ViewModels
{
    public class GameViewModel : BaseViewModel
    {
        private readonly GameService _gameService;
        private readonly DatabaseService _databaseService;
        private readonly InventoryService _inventoryService;
        private readonly PowerUpService _powerUpService;

        private int _currentCoins;
        private int _currentLevel;
        private int _currentScore;
        private int _currentLives;
        private int _currentFreezePower;
        private int _currentSlowPower;
        private int _currentAutoSortPower;
        private int _currentSpeedPower;
        private bool _isRecycleBinOpen;
        private bool _isBioBinOpen;
        private bool _isHazardBinOpen;
        private bool _isLandfillBinOpen;
        private bool _gameOverHandled;
        private bool _isPaused;
        private bool _isExiting;

        public bool IsPaused
        {
            get => _isPaused;
            private set => SetProperty(ref _isPaused, value);
        }

        public void PauseForAppInactive()
        {
            if (IsPaused || IsGameOver)
                return;

            IsPaused = true;
        }

        public int CurrentCoins
        {
            get => _currentCoins;
            private set => SetProperty(ref _currentCoins, value);
        }

        public int CurrentLevel
        {
            get => _currentLevel;
            private set => SetProperty(ref _currentLevel, value);
        }

        public int CurrentScore
        {
            get => _currentScore;
            private set => SetProperty(ref _currentScore, value);
        }

        public int CurrentLives
        {
            get => _currentLives;
            private set => SetProperty(ref _currentLives, value);
        }

        public int CurrentFreezePower
        {
            get => _currentFreezePower;
            private set => SetProperty(ref _currentFreezePower, value);
        }

        public int CurrentSlowPower
        {
            get => _currentSlowPower;
            private set => SetProperty(ref _currentSlowPower, value);
        }

        public int CurrentAutoSortPower
        {
            get => _currentAutoSortPower;
            private set => SetProperty(ref _currentAutoSortPower, value);
        }

        public int CurrentSpeedPower
        {
            get => _currentSpeedPower;
            private set => SetProperty(ref _currentSpeedPower, value);
        }

        public bool IsRecycleBinOpen
        {
            get => _isRecycleBinOpen;
            private set => SetProperty(ref _isRecycleBinOpen, value);
        }

        public bool IsBioBinOpen
        {
            get => _isBioBinOpen;
            private set => SetProperty(ref _isBioBinOpen, value);
        }

        public bool IsHazardBinOpen
        {
            get => _isHazardBinOpen;
            private set => SetProperty(ref _isHazardBinOpen, value);
        }

        public bool IsLandfillBinOpen
        {
            get => _isLandfillBinOpen;
            private set => SetProperty(ref _isLandfillBinOpen, value);
        }

        public bool IsGameOver => _gameService.IsGameOver;

        public bool IsPowerUpActive => _gameService.IsPowerUpActive;

        public string ActivePowerUpName => _gameService.ActivePowerUp?.Name ?? string.Empty;

        public string ActivePowerUpIcon => ActivePowerUpName switch
        {
            "Freeze" => "freeze02.png",
            "Slow" => "slow02.png",
            "Auto Sort" => "autosort02.png",
            "Speed" => "speed02.png",
            _ => string.Empty
        };

        public int ActivePowerUpRemainingSeconds => _gameService.ActivePowerUpRemainingSeconds;

        public string ActivePowerUpDisplayText =>
            IsPowerUpActive ? $"{ActivePowerUpName.ToUpper()} {ActivePowerUpRemainingSeconds}s" : string.Empty;

        public bool IsFreezeActive => ActivePowerUpName == "Freeze";
        public bool IsSlowActive => ActivePowerUpName == "Slow";
        public bool IsAutoSortActive => ActivePowerUpName == "Auto Sort";
        public bool IsSpeedActive => ActivePowerUpName == "Speed";

        public bool CanUseFreezePower => !IsPowerUpActive && CurrentFreezePower > 0;
        public bool CanUseSlowPower => !IsPowerUpActive && CurrentSlowPower > 0;
        public bool CanUseAutoSortPower => !IsPowerUpActive && CurrentAutoSortPower > 0;
        public bool CanUseSpeedPower => !IsPowerUpActive && CurrentSpeedPower > 0;

        public double FreezeButtonOpacity => IsFreezeActive || CanUseFreezePower ? 1.0 : 0.4;
        public double SlowButtonOpacity => IsSlowActive || CanUseSlowPower ? 1.0 : 0.4;
        public double AutoSortButtonOpacity => IsAutoSortActive || CanUseAutoSortPower ? 1.0 : 0.4;
        public double SpeedButtonOpacity => IsSpeedActive || CanUseSpeedPower ? 1.0 : 0.4;

        public ICommand UseFreezePowerCommand { get; }
        public ICommand UseSlowPowerCommand { get; }
        public ICommand UseAutoSortPowerCommand { get; }
        public ICommand UseSpeedPowerCommand { get; }
        public ICommand PauseGameCommand { get; }
        public ICommand ResumeGameCommand { get; }

        public event Action<int, string>? PointsEarned;

        public GameViewModel()
        {
            _databaseService = new DatabaseService();
            _powerUpService = new PowerUpService();
            _inventoryService = new InventoryService(_databaseService, _powerUpService);
            _gameService = new GameService(_databaseService, _inventoryService);
            _gameService.GameStateChanged += OnGameStateChanged;

            UseFreezePowerCommand = new Command(async () => await UsePowerUpAsync("Freeze"));
            UseSlowPowerCommand = new Command(async () => await UsePowerUpAsync("Slow"));
            UseAutoSortPowerCommand = new Command(async () => await UsePowerUpAsync("Auto Sort"));
            UseSpeedPowerCommand = new Command(async () => await UsePowerUpAsync("Speed"));
            PauseGameCommand = new Command(() => IsPaused = true);
            ResumeGameCommand = new Command(() => IsPaused = false);
        }

        public void PrepareToQuit()
        {
            if (_isExiting)
                return;

            _isExiting = true;
            _gameOverHandled = true;
            IsPaused = false;
            _gameService.ResetForExit();
        }

        public async Task InitializeAsync()
        {
            _isExiting = false;
            _gameOverHandled = false;
            IsPaused = false;
            await _powerUpService.InitializeAsync();
            await _gameService.StartGameAsync();
            await RefreshCoinsAsync();
            await RefreshPowerUpCountsAsync();
            SyncGameState();
        }

        public TrashItem? GetRandomTrash() => _gameService.GetRandomTrash();

        public double GetSpawnIntervalSeconds() => _gameService.SpawnInterval;

        public double GetFallSpeedPixelsPerSecond()
        {
            double speedMultiplier = _gameService.CurrentSpeedMultiplier;

            if (speedMultiplier <= 0)
                return 0;

            return _gameService.TrashSpeed * speedMultiplier * 80;
        }

        public bool IsTrashFrozen() => _gameService.CurrentSpeedMultiplier <= 0;

        public bool TryAutoSortTrash(TrashItem? trash, double trashBottomY, double arenaHeight)
        {
            int pointsEarned = _gameService.TryAutoSortTrash(trash, trashBottomY, arenaHeight);
            if (pointsEarned <= 0)
                return false;

            if (!string.IsNullOrWhiteSpace(trash?.Category))
            {
                PointsEarned?.Invoke(pointsEarned, trash.Category);
                _ = FlashBinAsync(trash.Category);
            }

            return true;
        }

        public bool TryManualSortTrash(
            TrashItem? trash,
            double trashCenterX,
            double trashBottomY,
            double arenaWidth,
            double arenaHeight)
        {
            (TrashSortOutcome outcome, int pointsEarned) = _gameService.TryManualSortTrash(
                trash,
                trashCenterX,
                trashBottomY,
                arenaWidth,
                arenaHeight);

            if (outcome == TrashSortOutcome.Correct
                && !string.IsNullOrWhiteSpace(trash?.Category))
            {
                PointsEarned?.Invoke(pointsEarned, trash.Category);
                _ = FlashBinAsync(trash.Category);
            }

            else if (outcome == TrashSortOutcome.Incorrect) 
            {
                _ = SettingsService.Instance.PlaySfxAsync("wrongbin.mp3");
            }

                return outcome != TrashSortOutcome.NotInBinZone;
        }

        public void OnTrashMissed()
        {
            if (IsGameOver)
                return;

            _gameService.LoseLife();
        }

        private void OnGameStateChanged()
        {
            if (Application.Current == null)
                return;

            MainThread.BeginInvokeOnMainThread(SyncGameState);
        }

        private void SyncGameState()
        {
            if (_isExiting)
                return;

            CurrentLevel = _gameService.DifficultyLevel;
            CurrentScore = _gameService.Score;
            CurrentLives = _gameService.Lives;
            OnPropertyChanged(nameof(IsGameOver));
            OnPropertyChanged(nameof(IsPowerUpActive));
            OnPropertyChanged(nameof(ActivePowerUpName));
            OnPropertyChanged(nameof(ActivePowerUpIcon));
            OnPropertyChanged(nameof(ActivePowerUpRemainingSeconds));
            OnPropertyChanged(nameof(ActivePowerUpDisplayText));
            OnPropertyChanged(nameof(IsFreezeActive));
            OnPropertyChanged(nameof(IsSlowActive));
            OnPropertyChanged(nameof(IsAutoSortActive));
            OnPropertyChanged(nameof(IsSpeedActive));
            NotifyPowerUpButtonState();

            if (_gameService.IsGameOver && !_gameOverHandled)
            {
                _gameOverHandled = true;
                _ = NavigateToGameOverAsync();
            }
        }

        private async Task NavigateToGameOverAsync()
        {
            if (Shell.Current == null)
                return;

            GameSession session = _gameService.EndGame();
            string route =
                $"GameOverPage?TotalScore={session.FinalScore}&EarnedCoins={session.CoinsEarned}";

            await Shell.Current.GoToAsync(route);
        }

        private void NotifyPowerUpButtonState()
        {
            OnPropertyChanged(nameof(CanUseFreezePower));
            OnPropertyChanged(nameof(CanUseSlowPower));
            OnPropertyChanged(nameof(CanUseAutoSortPower));
            OnPropertyChanged(nameof(CanUseSpeedPower));
            OnPropertyChanged(nameof(FreezeButtonOpacity));
            OnPropertyChanged(nameof(SlowButtonOpacity));
            OnPropertyChanged(nameof(AutoSortButtonOpacity));
            OnPropertyChanged(nameof(SpeedButtonOpacity));
        }

        private async Task RefreshCoinsAsync()
        {
            Player player = await _databaseService.GetPlayerAsync();
            CurrentCoins = player.Coins;
        }

        private async Task RefreshPowerUpCountsAsync()
        {
            CurrentFreezePower = await _inventoryService.GetPowerUpCountAsync("Freeze");
            CurrentSlowPower = await _inventoryService.GetPowerUpCountAsync("Slow");
            CurrentAutoSortPower = await _inventoryService.GetPowerUpCountAsync("Auto Sort");
            CurrentSpeedPower = await _inventoryService.GetPowerUpCountAsync("Speed");
            NotifyPowerUpButtonState();
        }

        private async Task UsePowerUpAsync(string powerUpName)
        {
            if (_gameService.IsPowerUpActive)
                return;

            if (await _inventoryService.GetPowerUpCountAsync(powerUpName) <= 0)
                return;

            if (_powerUpService.PowerUps.Count == 0)
                await _powerUpService.InitializeAsync();

            PowerUp? powerUp = _powerUpService.PowerUps.FirstOrDefault(p => p.Name == powerUpName);
            if (powerUp == null)
                return;

            bool used = await _inventoryService.UsePowerUpAsync(powerUpName);
            if (!used)
                return;

            await RefreshPowerUpCountsAsync();
            await _gameService.ActivatePowerUpAsync(powerUp);
        }

        private async Task FlashBinAsync(string category)
        {
            if (_isExiting)
                return;

            switch (category)
            {
                case "Recyclables":
                    IsRecycleBinOpen = true;
                    break;
                case "Biodegradable":
                    IsBioBinOpen = true;
                    break;
                case "Biohazard":
                    IsHazardBinOpen = true;
                    break;
                case "Landfill":
                    IsLandfillBinOpen = true;
                    break;
            }

            await Task.Delay(250);

            if (_isExiting)
                return;

            IsRecycleBinOpen = false;
            IsBioBinOpen = false;
            IsHazardBinOpen = false;
            IsLandfillBinOpen = false;
        }
    }
}
