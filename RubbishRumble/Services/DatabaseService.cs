using RubbishRumble.Models;
using SQLite;

namespace RubbishRumble.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? database;

        public async Task InitAsync()
        {
            if (database != null)
                return;

            string path = Path.Combine(FileSystem.AppDataDirectory, "rubbishrumble.db");
            database = new SQLiteAsyncConnection(path);

            await database.CreateTableAsync<Player>();
            await database.CreateTableAsync<GameSession>();
            await database.CreateTableAsync<Inventory>();

            Player? player = await database.FindAsync<Player>(1);

            if (player == null)
                await database.InsertAsync(new Player());
        }

        public async Task<Player> GetPlayerAsync()
        {
            await InitAsync();

            Player player = await database!.FindAsync<Player>(1);

            if (player == null)
            {
                player = new Player();
                await database.InsertAsync(player);
            }

            return player;
        }

        public async Task SavePlayerAsync(Player player)
        {
            await InitAsync();
            SQLiteAsyncConnection db = database!;

            if (player.Id == 0)
                await db.InsertAsync(player);
            else
                await db.UpdateAsync(player);
        }

        public async Task SaveGameSessionAsync(GameSession session)
        {
            await InitAsync();
            await database!.InsertAsync(session);
        }

        public async Task SaveInventoryAsync(Inventory inventory)
        {
            await InitAsync();
            SQLiteAsyncConnection db = database!;

            if (inventory.Id == 0)
                await db.InsertAsync(inventory);
            else
                await db.UpdateAsync(inventory);
        }

        public async Task<List<Inventory>> GetInventoryAsync()
        {
            await InitAsync();
            return await database!.Table<Inventory>().ToListAsync();
        }

        public async Task<int> AwardGameRewardsAsync(int finalScore, int coinsEarned)
        {
            await InitAsync();

            Player player = await GetPlayerAsync();
            player.Coins += coinsEarned;

            if (finalScore > player.HighestScore)
                player.HighestScore = finalScore;

            player.TotalGamesPlayed++;
            await SavePlayerAsync(player);

            await SaveGameSessionAsync(new GameSession
            {
                FinalScore = finalScore,
                CoinsEarned = coinsEarned,
                PlayedAt = DateTime.Now
            });

            return player.HighestScore;
        }
    }
}
