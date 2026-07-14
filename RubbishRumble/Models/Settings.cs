using SQLite;

namespace RubbishRumble.Models
{
    public class Settings
    {
        [PrimaryKey]
        public int Id { get; set; } = 1;
        public bool MusicEnabled { get; set; }
        public bool SoundEffectsEnabled { get; set; }
        public bool TutorialCompleted { get; set; }
        public int TotalCoins { get; set; }
    }
}
