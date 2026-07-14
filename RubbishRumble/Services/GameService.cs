using System.Text.Json;
using RubbishRumble.Models;
using RubbishRumble.Helper;

namespace RubbishRumble.Services
{
    public class GameService
    {
        //Start game - done
        //End game -done
        //Increase score -done
        //Lose life - done
        //Spawn trash - 
        //Activate power-ups
        //Increase difficulty

        //1. Load trashitems.json. - done
        //2. Load rarity.json. - done
        //3. Pick a rarity using Chance. - done
        //4. Find all trash items with that rarity.- done
        //5. Randomly choose one of those items. - unsure
        //6. Give the player the Points from that rarity. - unsure
        //7. Calculate coins. - unsure

        public int Score { get; private set; }
        public int Lives { get; private set; }
        public int Coins { get; private set; }
        // Basis of level increase
        public int DifficultyLevel { get; private set; }
        public int TrashCollected { get; private set; }

        // Data to be changed every level increase
        public double SpawnInterval { get; private set; }
        public double TrashSpeed { get; private set; }
        //public double RarityMultiplier { get; set; }

        private List<TrashItem> _trashItems = new();
        private List<Rarity> _rarities = new();

        // Load JSON files (DATA)
        public async Task LoadGameDataAsync()
        {
            await LoadTrashItemsAsync();
            await LoadRaritiesAsync();
        }

        private async Task LoadTrashItemsAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("TrashData/trashitems.json");
            using StreamReader reader = new(stream);

            string json = await reader.ReadToEndAsync();
            _trashItems = JsonSerializer.Deserialize<List<TrashItem>>(json) ?? new List<TrashItem>();
        }

        private async Task LoadRaritiesAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("TrashData/rarity.json");
            using StreamReader reader = new(stream);

            string json = await reader.ReadToEndAsync();

            _rarities = JsonSerializer.Deserialize<List<Rarity>>(json) ?? new List<Rarity>();
        }

        // After Loading trash data
        public TrashItem GetRandomTrash()
        {
            Rarity selectedRarity = GetRandomRarity();

            List<TrashItem> matchingTrash = _trashItems.Where(t => t.Rarity == selectedRarity.RarityName).ToList();

            if (matchingTrash.Count == 0)
                return null;

            int index = RandomGenerator.Next(0, matchingTrash.Count);

            return matchingTrash[index];
        }

        private Rarity GetRandomRarity()
        {
            double totalWeight = 0;
            foreach (Rarity rarity in _rarities)
            {
                totalWeight += GetAdjustedChance(rarity);
            }

            double roll = RandomGenerator.NextDouble() * totalWeight;
            double currentWeight = 0;

            foreach (Rarity rarity in _rarities)
            {
                currentWeight += GetAdjustedChance(rarity);
                if (roll <= currentWeight)
                    return rarity;
            }
            return _rarities.First();
        }

        private double GetAdjustedChance(Rarity rarity)
        {
            double multiplier = 1 + (DifficultyLevel - 1) * 0.1;

            return rarity.RarityName switch
            {
                "Common" => Math.Max(20, rarity.Chance / multiplier),
                "Rare" => rarity.Chance * multiplier,
                "Epic" => rarity.Chance * (multiplier + 0.2),
                "Legendary" => rarity.Chance * (multiplier + 0.4),
                _ => rarity.Chance
            };
            //if (rarity.RarityName == "Common")
            //    return rarity.Chance;
            //return rarity.Chance * RarityMultiplier;
        }

        // Game basic mechanic
        public bool IsGameOver => Lives <= 0;

        public async Task StartGameAsync()
        {
            await LoadGameDataAsync();
            Score = 0;
            Lives = Constants.STARTING_LIVES;
            TrashCollected = 0;
            DifficultyLevel = 1;

            SpawnInterval = Constants.STARTING_SPAWN_INTERVAL;

            TrashSpeed = Constants.STARTING_TRASH_SPEED;

            //RarityMultiplier = Constants.STARTING_RARITY_MULTIPLIER;
        }

        public void LoseLife()
        {
            Lives--;

            if (Lives <= 0)
            {
                EndGame();
            }
        }

        public int CalculateCoins()
        {
            // you can edit here how much score is converted into coins (i.e., Score/2 120 score = 60 coins)
            return Score;
        }
        public GameSession EndGame()
        {
            return new GameSession
            {
                // Total coins not added yet
                FinalScore = Score,
                CoinsEarned = CalculateCoins(),
                PlayedAt = DateTime.Now
            };
        }
        // Increase Difficulty
        
        public void IncreaseDifficulty()
        {
            DifficultyLevel++;

            SpawnInterval = Math.Max(Constants.MIN_SPAWN_INTERVAL, SpawnInterval - 0.1);
            TrashSpeed = Math.Min(Constants.MAX_TRASH_SPEED, TrashSpeed + Constants.TRASH_SPEED_INCREASE);
        }

        public void CollectTrash(TrashItem trash)
        {
            Rarity rarity = _rarities.FirstOrDefault(r => r.RarityName == trash.Rarity);

            if (rarity == null)
                return;

            int scoreEarned = rarity.PointValue;
            Score += scoreEarned;

            TrashCollected++;

            if (TrashCollected % 20 == 0)
            {
                IncreaseDifficulty();
            }

        }
    }
}
