using SQLite;

namespace RubbishRumble.Models
{
    public class Player
    {
        [PrimaryKey]
        public int Id { get; set; } = 1;
        public int Coins { get; set; }
        public int HighestScore { get; set; }
        public int TotalGamesPlayed { get; set; }
        public bool ReceivedBeginnerPack { get; set; }
        public int PendingLeaderboardScore { get; set; }
    }
}