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

        public ICommand PauseGameCommand { get; }
        public ICommand UseFreezePowerCommand { get; }
        public ICommand UseSlowPowerCommand { get; }
        public ICommand UseAutoSortPowerCommand { get; }
        public ICommand UseSpeedPowerCommand { get; }

        public GameViewModel()
        {
            _databaseService = new DatabaseService();
            _powerUpService = new PowerUpService();
            _inventoryService = new InventoryService(_databaseService, _powerUpService);
            _gameService = new GameService(_databaseService, _powerUpService, _inventoryService);
            _gameService.GameStateChanged += OnGameStateChanged;

            PauseGameCommand = new Command(OnPauseGameExecuted);
            UseFreezePowerCommand = new Command(async () => await UsePowerUpAsync("Freeze"));
            UseSlowPowerCommand = new Command(async () => await UsePowerUpAsync("Slow"));
            UseAutoSortPowerCommand = new Command(async () => await UsePowerUpAsync("Auto Sort"));
            UseSpeedPowerCommand = new Command(async () => await UsePowerUpAsync("Speed"));
        }

        public async Task InitializeAsync()
        {
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

        public void OnTrashSorted(TrashItem? trash)
        {
            if (IsGameOver || trash == null)
                return;

            _gameService.CollectTrash(trash);

            if (!string.IsNullOrWhiteSpace(trash.Category))
                _ = FlashBinAsync(trash.Category);
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
            CurrentLevel = _gameService.DifficultyLevel;
            CurrentScore = _gameService.Score;
            CurrentLives = _gameService.Lives;
            OnPropertyChanged(nameof(IsGameOver));
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
        }

        private async Task UsePowerUpAsync(string powerUpName)
        {
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

            IsRecycleBinOpen = false;
            IsBioBinOpen = false;
            IsHazardBinOpen = false;
            IsLandfillBinOpen = false;
        }

        private void OnPauseGameExecuted()
        {
        }
    }
}
