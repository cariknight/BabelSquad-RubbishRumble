using SQLite;

namespace RubbishRumble.Models
{
    public class LeaderboardEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int PlayerId { get; set; } = 1;

        public string PlayerName { get; set; } = string.Empty;

        public string AvatarId { get; set; } = string.Empty;

        public int HighestScore { get; set; }

        public DateTime AchievedAt { get; set; }
    }
}
