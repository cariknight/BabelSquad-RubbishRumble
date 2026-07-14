using System.Text.Json;
using RubbishRumble.Models;
using RubbishRumble.Helper;
using System.Threading.Tasks;



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

        //1. Load trashitems.json.
        //2. Load rarity.json.
        //3. Pick a rarity using Chance.
        //4. Find all trash items with that rarity.
        //5. Randomly choose one of those items.
        //6. Give the player the Points from that rarity.
        //7. Calculate coins.

        public int Score { get; private set; }
        public int Lives { get; private set; }
        public int Coins { get; private set; }
        public int DifficultyLevel { get; private set; }
        public double SpawnInterval { get; private set; }
        public double TrashSpeed { get; private set; }
        public double RarityMultiplier { get; set; }

        private List<TrashItem> _trashItems = new();
        private List<Rarity> _rarities = new();

        // Load JSON files
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

        // Game
        public bool IsGameOver => Lives <= 0;

        public async Task StartGameAsync()
        {
            await LoadGameDataAsync();
            Score = 0;
            Lives = Constants.STARTING_LIVES;

            DifficultyLevel = 1;

            SpawnInterval = Constants.STARTING_SPAWN_INTERVAL;

            TrashSpeed = Constants.STARTING_TRASH_SPEED;

            RarityMultiplier = Constants.STARTING_RARITY_MULTIPLIER;
        }

        public void AddScore(int points)
        {
            Score += points;
            IncreaseDifficulty();
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
        public void IncreaseDifficulty()
        {
            DifficultyLevel++;

            SpawnInterval = Math.Max(Constants.MIN_SPAWN_INTERVAL, SpawnInterval - 0.1);
            TrashSpeed = Math.Min(Constants.MAX_TRASH_SPEED, TrashSpeed + Constants.SPAWN_INTERVAL_DECREASE);
            RarityMultiplier = Math.Min(Constants.MAX_RARITY_MULTIPLIER, RarityMultiplier + Constants.RARITY_MULTIPLIER_INCREASE);
        }

        public void CollectTrash(TrashItem trash, Rarity rarity)
        {
            int scoreEarned = rarity.PointValue;
            int coinsEarned = rarity.PointValue * 2;
            Score += scoreEarned;

        }
    }
}
