using RubbishRumble.Helper;
using RubbishRumble.Models;

namespace RubbishRumble.Services
{
    public class LeaderboardService
    {
        private const int DefaultLimit = 10;
        private const int MaxPlayerNameLength = 20;

        private readonly DatabaseService _database;

        public LeaderboardService(DatabaseService database)
        {
            _database = database;
        }

        public LeaderboardService() : this(new DatabaseService())
        {
        }

        public IReadOnlyList<AvatarOption> GetAvailableAvatars() =>
            AvatarHelper.GetAvailableAvatars();

        public async Task<IReadOnlyList<LeaderboardEntry>> GetLeaderboardAsync(int limit = DefaultLimit)
        {
            if (limit <= 0)
                return Array.Empty<LeaderboardEntry>();

            return await _database.GetLeaderboardEntriesAsync(limit);
        }

        public async Task<int> GetPendingLeaderboardScoreAsync()
        {
            Player player = await _database.GetPlayerAsync();
            return player.PendingLeaderboardScore;
        }

        public async Task<bool> CanSubmitLeaderboardEntryAsync(int score)
        {
            if (score <= 0)
                return false;

            Player player = await _database.GetPlayerAsync();
            return score == player.PendingLeaderboardScore;
        }

        public async Task<LeaderboardSubmitResult> SubmitLeaderboardEntryAsync(
            string playerName,
            string avatarId,
            int score)
        {
            string trimmedName = playerName?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(trimmedName))
                return LeaderboardSubmitResult.Failed("Player name is required.");

            if (trimmedName.Length > MaxPlayerNameLength)
                return LeaderboardSubmitResult.Failed($"Player name must be {MaxPlayerNameLength} characters or fewer.");

            if (!AvatarHelper.IsValidAvatarId(avatarId))
                return LeaderboardSubmitResult.Failed("Invalid avatar selection.");

            if (score <= 0)
                return LeaderboardSubmitResult.Failed("Score must be greater than zero.");

            Player player = await _database.GetPlayerAsync();

            if (score != player.PendingLeaderboardScore)
                return LeaderboardSubmitResult.Failed("Leaderboard entries can only be submitted after beating the high score.");

            LeaderboardEntry entry = await _database.SaveLeaderboardEntryAsync(new LeaderboardEntry
            {
                PlayerName = trimmedName,
                AvatarId = avatarId,
                HighestScore = score,
                AchievedAt = DateTime.Now
            });

            await _database.ClearPendingLeaderboardScoreAsync();

            return LeaderboardSubmitResult.Succeeded(entry);
        }

        public string GetAvatarImagePath(string avatarId) =>
            AvatarHelper.GetAvatarImagePath(avatarId);
    }
}
