using SQLite;

namespace RubbishRumble.Models
{
    public class GameSession
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int PlayerId { get; set; } = 1;

        public int FinalScore { get; set; }
        public int CoinsEarned { get; set; }
        public DateTime PlayedAt { get; set; }
    }
}
