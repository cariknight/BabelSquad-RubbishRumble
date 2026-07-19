namespace RubbishRumble.Models
{
    public class LeaderboardSubmitResult
    {
        public bool Success { get; init; }

        public string ErrorMessage { get; init; } = string.Empty;

        public LeaderboardEntry? Entry { get; init; }

        public static LeaderboardSubmitResult Succeeded(LeaderboardEntry entry) =>
            new() { Success = true, Entry = entry };

        public static LeaderboardSubmitResult Failed(string errorMessage) =>
            new() { Success = false, ErrorMessage = errorMessage };
    }
}
