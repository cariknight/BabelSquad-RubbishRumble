using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RubbishRumble.Models;
using SQLite;

namespace RubbishRumble.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection database;

        public async Task InitAsync()
        {
            if (database != null)
                return;

            string path = Path.Combine(FileSystem.AppDataDirectory, "rubbishrumble.db");
            database = new SQLiteAsyncConnection(path);

            await database.CreateTableAsync<Player>();
            await database.CreateTableAsync<GameSession>();
            await database.CreateTableAsync<Inventory>();
            await database.CreateTableAsync<Settings>();

            Player? player = await database.FindAsync<Player>(1);

            if (player == null)
            {
                await database.InsertAsync(new Player());
            }
        }

        public async Task<Player> GetPlayerAsync()
        {
            await InitAsync();

            Player player = await database.FindAsync<Player>(1);

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
            if (player.Id == 0)
            {
                await database.InsertAsync(player);
            }
            else
            {
                await database.UpdateAsync(player);
            }
        }
        public async Task SaveGameSessionAsync(GameSession session)
        {
            await InitAsync();
            await database.InsertAsync(session);
        }

        public async Task<List<GameSession>> GetGameSessionsAsync()
        {
            await InitAsync();

            return await database.Table<GameSession>().OrderByDescending(g=>g.PlayedAt).ToListAsync();
        }

        public async Task SaveInventoryAsync(Inventory inventory)
        {
            await InitAsync();
            if (inventory.Id == 0)
            {
                await database.InsertAsync(inventory);
            }
            else
            {
                await database.UpdateAsync(inventory);
            }
        }
        public async Task<List<Inventory>> GetInventoryAsync()
        {
            await InitAsync();

            return await database.Table<Inventory>().ToListAsync();
        }

        public async Task<Settings> GetSettingsAsync()
        {
            await InitAsync();
            Settings settings = await database.Table<Settings>().FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new Settings();

                await database.InsertAsync(settings);
            }
            return settings;
        }

        public async Task SaveSettingsAsync(Settings settings)
        {
            await InitAsync();

            if (settings.Id == 0)
            {
                await database.InsertAsync(settings);
            }
            else
            {
                await database.UpdateAsync(settings);
            }
        }
    }
}
