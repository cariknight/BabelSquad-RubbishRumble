using RubbishRumble.Models;

namespace RubbishRumble.Services
{
    public class InventoryService
    {
        private const int BeginnerPackQuantity = 3;

        private static readonly string[] DefaultPowerUpNames =
        {
            "Freeze",
            "Slow",
            "Auto Sort",
            "Speed"
        };

        private readonly DatabaseService _database;
        private readonly PowerUpService _powerUpService;

        public InventoryService(DatabaseService database, PowerUpService powerUpService)
        {
            _database = database;
            _powerUpService = powerUpService;
        }

        public async Task ApplyBeginnerPackAsync(Player player)
        {
            if (player.ReceivedBeginnerPack)
                return;

            IEnumerable<string> powerUpNames = _powerUpService.PowerUps.Count > 0
                ? _powerUpService.PowerUps.Select(p => p.Name)
                : DefaultPowerUpNames;

            foreach (string powerUpName in powerUpNames)
                await AddPowerUpAsync(powerUpName, BeginnerPackQuantity);

            player.ReceivedBeginnerPack = true;
            await _database.SavePlayerAsync(player);
        }

        public async Task AddPowerUpAsync(string powerUpName, int amount)
        {
            List<Inventory> inventory = await _database.GetInventoryAsync();
            Inventory? item = inventory.FirstOrDefault(i => i.PowerUpName == powerUpName);

            if (item == null)
            {
                await _database.SaveInventoryAsync(new Inventory
                {
                    PowerUpName = powerUpName,
                    Quantity = amount
                });
            }
            else
            {
                item.Quantity += amount;
                await _database.SaveInventoryAsync(item);
            }
        }

        public async Task<bool> UsePowerUpAsync(string powerUpName)
        {
            List<Inventory> inventory = await _database.GetInventoryAsync();
            Inventory? item = inventory.FirstOrDefault(i => i.PowerUpName == powerUpName);

            if (item == null || item.Quantity <= 0)
                return false;

            item.Quantity--;
            await _database.SaveInventoryAsync(item);
            return true;
        }

        public async Task<int> GetPowerUpCountAsync(string powerUpName)
        {
            List<Inventory> inventory = await _database.GetInventoryAsync();
            Inventory? item = inventory.FirstOrDefault(i => i.PowerUpName == powerUpName);

            return item?.Quantity ?? 0;
        }
    }
}
