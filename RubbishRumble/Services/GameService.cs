using System.Text.Json;
using RubbishRumble.Helper;
using RubbishRumble.Models;

namespace RubbishRumble.Services
{
    public class GameService
    {
        private readonly DatabaseService _dataService;
        private readonly InventoryService _inventoryService;

        public GameService(
            DatabaseService dataService,
            InventoryService inventoryService)
        {
            _dataService = dataService;
            _inventoryService = inventoryService;
        }

        public int Score { get; private set; }
        public int Lives { get; private set; }
        public int DifficultyLevel { get; private set; }
        public int TrashCollected { get; private set; }
        public double SpawnInterval { get; private set; }
        public double TrashSpeed { get; private set; }

        private List<TrashItem> _trashItems = new();
        private List<Rarity> _rarities = new();
        private int _trashSinceLastDifficultyIncrease;

        public double CurrentScoreMultiplier { get; private set; } = 1.0;
        public double CurrentSpeedMultiplier { get; private set; } = 1.0;
        public bool IsAutoSortActive { get; private set; }
        public PowerUp? ActivePowerUp { get; private set; }
        public int ActivePowerUpRemainingSeconds { get; private set; }
        public bool IsPowerUpActive => ActivePowerUp != null;

        private CancellationTokenSource? _powerUpCts;

        public event Action? GameStateChanged;

        private void NotifyGameStateChanged() => GameStateChanged?.Invoke();

        public async Task LoadGameDataAsync()
        {
            await LoadTrashItemsAsync();
            await LoadRaritiesAsync();
        }

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private async Task LoadTrashItemsAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("TrashData/trashitems.json");
            using StreamReader reader = new(stream);

            string json = await reader.ReadToEndAsync();
            _trashItems = JsonSerializer.Deserialize<List<TrashItem>>(json, JsonOptions) ?? new List<TrashItem>();
        }

        private async Task LoadRaritiesAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("TrashData/rarity.json");
            using StreamReader reader = new(stream);

            string json = await reader.ReadToEndAsync();

            _rarities = JsonSerializer.Deserialize<List<Rarity>>(json, JsonOptions) ?? new List<Rarity>();
        }

        public TrashItem? GetRandomTrash()
        {
            if (_trashItems.Count == 0 || _rarities.Count == 0)
                return null;

            Rarity? selectedRarity = GetRandomRarity();
            if (selectedRarity == null || string.IsNullOrWhiteSpace(selectedRarity.RarityName))
                return null;

            List<TrashItem> matchingTrash = _trashItems
                .Where(t => t != null && t.Rarity == selectedRarity.RarityName)
                .ToList();

            if (matchingTrash.Count == 0)
                return null;

            int index = RandomGenerator.Next(0, matchingTrash.Count);
            return matchingTrash[index];
        }

        private Rarity? GetRandomRarity()
        {
            if (_rarities.Count == 0)
                return null;

            double totalWeight = 0;
            foreach (Rarity? rarity in _rarities)
            {
                if (rarity != null)
                    totalWeight += GetAdjustedChance(rarity);
            }

            if (totalWeight <= 0)
                return _rarities.FirstOrDefault();

            double roll = RandomGenerator.NextDouble() * totalWeight;
            double currentWeight = 0;

            foreach (Rarity? rarity in _rarities)
            {
                if (rarity == null)
                    continue;

                currentWeight += GetAdjustedChance(rarity);
                if (roll <= currentWeight)
                    return rarity;
            }

            return _rarities.FirstOrDefault();
        }

        private double GetAdjustedChance(Rarity? rarity)
        {
            if (rarity == null)
                return 0;

            double multiplier = 1 + (DifficultyLevel - 1) * 0.1;

            return rarity.RarityName switch
            {
                "Common" => Math.Max(20, rarity.Chance / multiplier),
                "Uncommon" => Math.Max(10, rarity.Chance / multiplier),
                "Rare" => rarity.Chance * multiplier,
                "Epic" => rarity.Chance * (multiplier + 0.2),
                _ => rarity.Chance
            };
        }

        public bool IsGameOver => Lives <= 0;

        public async Task<bool> CanReviveAsync()
        {
            if (!IsGameOver)
                return false;

            return await _inventoryService.GetPowerUpCountAsync("Revive") > 0;
        }

        public async Task<bool> TryReviveAsync()
        {
            if (!IsGameOver)
                return false;

            if (!await _inventoryService.UsePowerUpAsync("Revive"))
                return false;

            Revive();
            return true;
        }

        public void Revive()
        {
            if (!IsGameOver)
                return;

            Lives = Constants.STARTING_LIVES;
            NotifyGameStateChanged();
        }

        public async Task StartGameAsync()
        {
            Player player = await _dataService.GetPlayerAsync();

            await _inventoryService.ApplyBeginnerPackAsync(player);
            await _inventoryService.EnsureBeginnerRevivesAsync(player);

            await LoadGameDataAsync();

            Score = 0;
            Lives = Constants.STARTING_LIVES;
            TrashCollected = 0;
            _trashSinceLastDifficultyIncrease = 0;
            DifficultyLevel = 1;
            ApplySpawnRateForCurrentLevel();
            TrashSpeed = Constants.STARTING_TRASH_SPEED;

            ClearPowerUpState();

            NotifyGameStateChanged();
        }

        public void LoseLife()
        {
            if (Lives <= 0)
                return;

            Lives--;

            NotifyGameStateChanged();
        }

        public GameSession EndGame(bool isNewHighScore)
        {
            return new GameSession
            {
                FinalScore = Score,
                CoinsEarned = EconomyHelper.CalculateEarnedCoins(Score, isNewHighScore),
                PlayedAt = DateTime.Now
            };
        }

        public void IncreaseDifficulty()
        {
            DifficultyLevel++;

            ApplySpawnRateForCurrentLevel();
            TrashSpeed = Math.Min(Constants.MAX_TRASH_SPEED, TrashSpeed + Constants.TRASH_SPEED_INCREASE);

            NotifyGameStateChanged();
        }

        private void ApplySpawnRateForCurrentLevel()
        {
            double interval = Constants.STARTING_SPAWN_INTERVAL
                - (DifficultyLevel - 1) * Constants.SPAWN_INTERVAL_DECREASE;

            SpawnInterval = Math.Max(Constants.MIN_SPAWN_INTERVAL, interval);
        }

        private int GetTrashRequiredForNextLevel()
            => DifficultyLevel * Constants.DIFFICULTY_TRASH_BASE;

        private void TryIncreaseDifficulty()
        {
            int required = GetTrashRequiredForNextLevel();
            while (_trashSinceLastDifficultyIncrease >= required)
            {
                _trashSinceLastDifficultyIncrease -= required;
                IncreaseDifficulty();
                required = GetTrashRequiredForNextLevel();
            }
        }

        private double GetDifficultyScoreMultiplier() => 1 + (DifficultyLevel - 1) * 0.1;

        public int CollectTrash(TrashItem? trash)
        {
            if (trash == null)
                return 0;

            Rarity? rarity = _rarities.FirstOrDefault(r => r != null && r.RarityName == trash.Rarity);

            if (rarity == null)
                return 0;

            int scoreEarned = (int)Math.Round(rarity.PointValue * CurrentScoreMultiplier * GetDifficultyScoreMultiplier());
            Score += scoreEarned;

            TrashCollected++;
            _trashSinceLastDifficultyIncrease++;
            TryIncreaseDifficulty();

            NotifyGameStateChanged();
            return scoreEarned;
        }

        public bool IsTrashInBinZone(double trashBottomY, double arenaHeight)
            => arenaHeight > 0 && trashBottomY >= arenaHeight * Constants.BIN_ZONE_START_RATIO;

        public string? GetBinCategoryAtPosition(
            double trashCenterX,
            double trashBottomY,
            double arenaWidth,
            double arenaHeight)
        {
            if (arenaWidth <= 0 || arenaHeight <= 0)
                return null;

            if (!IsTrashInBinZone(trashBottomY, arenaHeight))
                return null;

            double ratio = trashCenterX / arenaWidth;

            if (ratio < 0.25)
                return "Recyclables";

            if (ratio < 0.5)
                return "Biodegradable";

            if (ratio < 0.75)
                return "Biohazard";

            return "Landfill";
        }

        public int TryAutoSortTrash(TrashItem? trash, double trashBottomY, double arenaHeight)
        {
            if (!IsAutoSortActive || IsGameOver || trash == null)
                return 0;

            if (!IsTrashInBinZone(trashBottomY, arenaHeight))
                return 0;

            return CollectTrash(trash);
        }

        public (TrashSortOutcome Outcome, int PointsEarned) TryManualSortTrash(
            TrashItem? trash,
            double trashCenterX,
            double trashBottomY,
            double arenaWidth,
            double arenaHeight)
        {
            if (IsGameOver || trash == null)
                return (TrashSortOutcome.NotInBinZone, 0);

            string? droppedCategory = GetBinCategoryAtPosition(
                trashCenterX,
                trashBottomY,
                arenaWidth,
                arenaHeight);

            if (droppedCategory == null)
                return (TrashSortOutcome.NotInBinZone, 0);

            string itemCategory = trash.Category ?? string.Empty;

            if (droppedCategory == itemCategory)
            {
                int pointsEarned = CollectTrash(trash);
                return (TrashSortOutcome.Correct, pointsEarned);
            }

            LoseLife();
            return (TrashSortOutcome.Incorrect, 0);
        }

        public async Task ActivatePowerUpAsync(PowerUp? powerUp)
        {
            if (powerUp == null || powerUp.EffectType == "Revive" || powerUp.DurationSeconds <= 0)
                return;

            _powerUpCts?.Cancel();
            _powerUpCts?.Dispose();
            _powerUpCts = new CancellationTokenSource();
            CancellationToken token = _powerUpCts.Token;

            ApplyPowerUpEffect(powerUp);
            NotifyGameStateChanged();

            try
            {
                while (ActivePowerUpRemainingSeconds > 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), token);

                    if (token.IsCancellationRequested)
                        return;

                    ActivePowerUpRemainingSeconds--;
                    NotifyGameStateChanged();
                }
            }
            catch (TaskCanceledException)
            {
                return;
            }

            ClearPowerUpState();
            NotifyGameStateChanged();
        }

        private void ApplyPowerUpEffect(PowerUp powerUp)
        {
            ActivePowerUp = powerUp;
            ActivePowerUpRemainingSeconds = powerUp.DurationSeconds;
            CurrentScoreMultiplier = powerUp.ScoreMultiplier;
            CurrentSpeedMultiplier = powerUp.SpeedMultiplier;
            IsAutoSortActive = powerUp.EffectType == "AutoSort";
        }

        public void ResetForExit()
        {
            ClearPowerUpState();
        }

        private void ClearPowerUpState()
        {
            _powerUpCts?.Cancel();
            _powerUpCts?.Dispose();
            _powerUpCts = null;

            ActivePowerUp = null;
            ActivePowerUpRemainingSeconds = 0;
            CurrentScoreMultiplier = 1.0;
            CurrentSpeedMultiplier = 1.0;
            IsAutoSortActive = false;
        }
    }
}
