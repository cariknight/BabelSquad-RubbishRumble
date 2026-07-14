using RubbishRumble.Models;

namespace RubbishRumble.Services
{
    public class InventoryService
    {
        // Beinner Pack
        public void AddPowerUp(Player player, string powerUpId, int amount)
        {
            var item = player.Inventory
                .FirstOrDefault(i => i.PowerUpId == powerUpId);

            if (item == null)
            {
                player.Inventory.Add(new InventoryItem
                {
                    PowerUpId = powerUpId,
                    Quantity = amount
                });
            }
            else
            {
                item.Quantity += amount;
            }

        }

        public bool UsePowerUp(Player player, string powerUpId)
        {
            var item = player.Inventory
                .FirstOrDefault(i => i.PowerUpId == powerUpId);

            if (item == null || item.Quantity <= 0)
                return false;

            item.Quantity--;

            return true;
        }

        public int GetPowerUpCount(Player player, string powerUpId)
        {
            var item = player.Inventory
                .FirstOrDefault(i => i.PowerUpId == powerUpId);

            return item?.Quantity ?? 0;
        }
    }
}
