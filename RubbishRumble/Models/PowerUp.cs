using SQLite;

namespace RubbishRumble.Models
{
    public class PowerUp
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed(Name = "IX_PowerUp_Name", Unique = true)]
        public string Name { get; set; } = string.Empty;

        public string EffectType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationSeconds { get; set; }
        public double ScoreMultiplier { get; set; }
        public double SpeedMultiplier { get; set; }
        public int Price { get; set; }
    }
}
