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

        }

        public async Task<List<Player>> GetPlayersAsync()
        {

        }

        public async Task SavePlayerAsync(Player player)
        {

        }

        public async Task SaveGameSessionAsync(GameSession session)
        {

        }
    }
}
