namespace RubbishRumble.Models
{
    public class Player
    {
        public int Id { get; set; } = 1;
        public int Coins { get; set; }
        public int HighestScore { get; set; }
        public int TotalGamesPlayed { get; set; }
        public bool ReceivedBeginnerPack { get; set; }
        public List<Inventory> Inventory { get; set; } = new();
    }
}