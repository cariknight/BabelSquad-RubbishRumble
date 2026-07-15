using SQLite;

namespace RubbishRumble.Models
{
    public class Inventory
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed(Name = "IX_Inventory_Player_PowerUp", Unique = true, Order = 1)]
        public int PlayerId { get; set; } = 1;

        [Indexed(Name = "IX_Inventory_Player_PowerUp", Unique = true, Order = 2)]
        public int PowerUpId { get; set; }

        public int Quantity { get; set; }
    }
}
