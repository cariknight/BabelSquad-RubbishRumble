using SQLite;

namespace RubbishRumble.Models
{
    public class PowerUp
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string EffectType { get; set; }
        public string Description { get; set; }
        public int DurationSeconds { get; set; }
        public double ScoreMultiplier { get; set; }
        public double SpeedMultiplier { get; set; }
        public int Price { get; set; }
    }
}
