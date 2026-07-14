using SQLite;

namespace RubbishRumble.Models
{
    public class Inventory
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string PowerUpName { get; set; }
        public int Quantity { get; set; }
    }
}
