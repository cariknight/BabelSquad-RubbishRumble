using RubbishRumble.Models;
using SQLite;
using System.Text.Json;

namespace RubbishRumble.Services
{
    public class DatabaseService
    {
        public const int DefaultPlayerId = 1;

        private static readonly SemaphoreSlim InitLock = new(1, 1);
        private static SQLiteAsyncConnection? SharedDatabase;

        private SQLiteAsyncConnection? database;

        public async Task InitAsync()
        {
            if (database != null)
                return;

            await InitLock.WaitAsync();
            try
            {
                if (SharedDatabase == null)
                {
                    string path = Path.Combine(FileSystem.AppDataDirectory, "rubbishrumble.db");
                    SharedDatabase = new SQLiteAsyncConnection(path);
                    database = SharedDatabase;

                    await SharedDatabase.CreateTableAsync<Player>();
                    await SharedDatabase.CreateTableAsync<PowerUp>();
                    await SharedDatabase.CreateTableAsync<GameSession>();
                    await SharedDatabase.CreateTableAsync<Inventory>();
                    await SharedDatabase.CreateTableAsync<LeaderboardEntry>();

                    await SeedPowerUpsAsync(SharedDatabase);
                    await MigrateLegacySchemaAsync(SharedDatabase);
                }
                else
                {
                    database = SharedDatabase;
                }

                Player? player = await database.FindAsync<Player>(DefaultPlayerId);

                if (player == null)
                    await database.InsertAsync(new Player { Id = DefaultPlayerId });
            }
            finally
            {
                InitLock.Release();
            }
        }

        public async Task<Player> GetPlayerAsync()
        {
            await InitAsync();

            Player player = await database!.FindAsync<Player>(DefaultPlayerId);

            if (player == null)
            {
                player = new Player { Id = DefaultPlayerId };
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

            if (session.PlayerId == 0)
                session.PlayerId = DefaultPlayerId;

            await database!.InsertAsync(session);
        }

        public async Task SaveInventoryAsync(Inventory inventory)
        {
            await InitAsync();
            SQLiteAsyncConnection db = database!;

            if (inventory.PlayerId == 0)
                inventory.PlayerId = DefaultPlayerId;

            if (inventory.Id == 0)
                await db.InsertAsync(inventory);
            else
                await db.UpdateAsync(inventory);
        }

        public async Task<Inventory?> GetInventoryItemAsync(string powerUpName, int playerId = DefaultPlayerId)
        {
            await InitAsync();

            int? powerUpId = await GetPowerUpIdByNameAsync(powerUpName);

            if (powerUpId == null)
                return null;

            return await database!.Table<Inventory>()
                .FirstOrDefaultAsync(i => i.PlayerId == playerId && i.PowerUpId == powerUpId.Value);
        }

        public async Task<int?> GetPowerUpIdByNameAsync(string powerUpName)
        {
            await InitAsync();

            PowerUp? powerUp = await database!.Table<PowerUp>()
                .FirstOrDefaultAsync(p => p.Name == powerUpName);

            return powerUp?.Id;
        }

        public async Task<int> AwardGameRewardsAsync(int finalScore, int coinsEarned)
        {
            await InitAsync();

            Player player = await GetPlayerAsync();
            int previousHighScore = player.HighestScore;
            player.Coins += coinsEarned;

            if (finalScore > player.HighestScore)
                player.HighestScore = finalScore;

            player.PendingLeaderboardScore = finalScore > previousHighScore
                ? finalScore
                : 0;

            player.TotalGamesPlayed++;
            await SavePlayerAsync(player);

            await SaveGameSessionAsync(new GameSession
            {
                PlayerId = DefaultPlayerId,
                FinalScore = finalScore,
                CoinsEarned = coinsEarned,
                PlayedAt = DateTime.Now
            });

            return player.HighestScore;
        }

        public async Task<IReadOnlyList<LeaderboardEntry>> GetLeaderboardEntriesAsync(int limit)
        {
            await InitAsync();

            List<LeaderboardEntry> entries = await database!.Table<LeaderboardEntry>()
                .OrderByDescending(e => e.HighestScore)
                .ThenByDescending(e => e.AchievedAt)
                .Take(limit)
                .ToListAsync();

            return entries;
        }

        public async Task ClearPendingLeaderboardScoreAsync()
        {
            await InitAsync();

            Player player = await GetPlayerAsync();
            player.PendingLeaderboardScore = 0;
            await SavePlayerAsync(player);
        }

        public async Task<LeaderboardEntry> SaveLeaderboardEntryAsync(LeaderboardEntry entry)
        {
            await InitAsync();

            if (entry.PlayerId == 0)
                entry.PlayerId = DefaultPlayerId;

            await database!.InsertAsync(entry);
            return entry;
        }

        private async Task SeedPowerUpsAsync(SQLiteAsyncConnection db)
        {
            List<PowerUp> catalog = await LoadPowerUpCatalogAsync();
            HashSet<string> seededNames = new(StringComparer.OrdinalIgnoreCase);

            foreach (PowerUp powerUp in catalog)
            {
                if (!seededNames.Add(powerUp.Name))
                    continue;

                PowerUp? existing = await db.Table<PowerUp>()
                    .FirstOrDefaultAsync(p => p.Name == powerUp.Name);

                if (existing == null)
                    await db.InsertAsync(powerUp);
            }
        }

        private static async Task<List<PowerUp>> LoadPowerUpCatalogAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("powerup.json");
            using StreamReader reader = new(stream);
            string json = await reader.ReadToEndAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<List<PowerUp>>(json, options) ?? new List<PowerUp>();
        }

        private async Task MigrateLegacySchemaAsync(SQLiteAsyncConnection db)
        {
            await MigrateInventoryAsync(db);
            await MigrateGameSessionsAsync(db);
            await MigrateLeaderboardPendingAsync(db);
        }

        private async Task MigrateInventoryAsync(SQLiteAsyncConnection db)
        {
            HashSet<string> columns = await GetTableColumnsAsync(db, "Inventory");

            if (columns.Contains("PowerUpName") && !columns.Contains("PowerUpId"))
            {
                await db.ExecuteAsync(
                    "ALTER TABLE Inventory ADD COLUMN PlayerId INTEGER NOT NULL DEFAULT 1");
                await db.ExecuteAsync(
                    "ALTER TABLE Inventory ADD COLUMN PowerUpId INTEGER NOT NULL DEFAULT 0");

                List<LegacyInventoryRow> legacyRows = await db.QueryAsync<LegacyInventoryRow>(
                    "SELECT Id, PowerUpName, Quantity FROM Inventory");

                await db.ExecuteAsync("DELETE FROM Inventory");

                foreach (LegacyInventoryRow row in legacyRows)
                {
                    int? powerUpId = await GetPowerUpIdByNameAsync(row.PowerUpName);

                    if (powerUpId == null)
                        continue;

                    await db.InsertAsync(new Inventory
                    {
                        PlayerId = DefaultPlayerId,
                        PowerUpId = powerUpId.Value,
                        Quantity = row.Quantity
                    });
                }

                return;
            }

            if (columns.Contains("PowerUpId"))
            {
                await db.ExecuteAsync(
                    "UPDATE Inventory SET PlayerId = ? WHERE PlayerId = 0 OR PlayerId IS NULL",
                    DefaultPlayerId);
            }
        }

        private async Task MigrateGameSessionsAsync(SQLiteAsyncConnection db)
        {
            HashSet<string> columns = await GetTableColumnsAsync(db, "GameSession");

            if (!columns.Contains("PlayerId"))
            {
                await db.ExecuteAsync(
                    "ALTER TABLE GameSession ADD COLUMN PlayerId INTEGER NOT NULL DEFAULT 1");
            }
            else
            {
                await db.ExecuteAsync(
                    "UPDATE GameSession SET PlayerId = ? WHERE PlayerId = 0 OR PlayerId IS NULL",
                    DefaultPlayerId);
            }
        }

        private async Task MigrateLeaderboardPendingAsync(SQLiteAsyncConnection db)
        {
            HashSet<string> columns = await GetTableColumnsAsync(db, "Player");

            if (!columns.Contains("PendingLeaderboardScore"))
            {
                await db.ExecuteAsync(
                    "ALTER TABLE Player ADD COLUMN PendingLeaderboardScore INTEGER NOT NULL DEFAULT 0");
            }
        }

        private async Task<HashSet<string>> GetTableColumnsAsync(SQLiteAsyncConnection db, string tableName)
        {
            List<ColumnInfo> columns = await db.QueryAsync<ColumnInfo>(
                $"PRAGMA table_info({tableName})");

            return columns
                .Select(c => c.name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private class LegacyInventoryRow
        {
            public int Id { get; set; }
            public string PowerUpName { get; set; } = string.Empty;
            public int Quantity { get; set; }
        }

        private class ColumnInfo
        {
            public string name { get; set; } = string.Empty;
        }
    }
}
