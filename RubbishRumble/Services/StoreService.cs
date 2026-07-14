using RubbishRumble.Models;

namespace RubbishRumble.Services
{
    public class StoreService
    {
        private readonly PowerUpService _powerUpService;
        private readonly InventoryService _inventoryService;
        private readonly DatabaseService _database;

        public StoreService(
            PowerUpService powerUpService,
            InventoryService inventoryService,
            DatabaseService database)
        {
            _powerUpService = powerUpService;
            _inventoryService = inventoryService;
            _database = database;
        }

        public async Task<IReadOnlyList<StoreItem>> GetStoreItemsAsync()
        {
            await EnsurePowerUpsLoadedAsync();

            return _powerUpService.PowerUps
                .Select(p => new StoreItem
                {
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price
                })
                .ToList();
        }

        public async Task<bool> BuyPowerUpAsync(string powerUpName, int quantity = 1)
        {
            if (quantity <= 0)
                return false;

            await EnsurePowerUpsLoadedAsync();

            PowerUp? powerUp = _powerUpService.PowerUps
                .FirstOrDefault(p => p.Name == powerUpName);

            if (powerUp == null)
                return false;

            int totalCost = powerUp.Price * quantity;
            Player player = await _database.GetPlayerAsync();

            if (player.Coins < totalCost)
                return false;

            player.Coins -= totalCost;
            await _database.SavePlayerAsync(player);
            await _inventoryService.AddPowerUpAsync(powerUp.Name, quantity);

            return true;
        }

        public async Task<int> GetPlayerCoinsAsync()
        {
            Player player = await _database.GetPlayerAsync();
            return player.Coins;
        }

        private async Task EnsurePowerUpsLoadedAsync()
        {
            if (_powerUpService.PowerUps.Count == 0)
                await _powerUpService.InitializeAsync();
        }
    }
}
