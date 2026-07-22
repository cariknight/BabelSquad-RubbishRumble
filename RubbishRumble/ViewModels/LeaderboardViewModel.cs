using System.Collections.ObjectModel;
using RubbishRumble.Models;
using RubbishRumble.Services;

namespace RubbishRumble.ViewModels
{
    public class LeaderboardViewModel : BaseViewModel
    {
        private readonly LeaderboardService _leaderboardService = new();

        public ObservableCollection<UserProfileModel> SavedProfiles { get; } = new();

        public async Task InitializeAsync()
        {
            await LoadLeaderboardAsync();
        }

        private async Task LoadLeaderboardAsync()
        {
            IReadOnlyList<LeaderboardEntry> entries = await _leaderboardService.GetLeaderboardAsync();

            SavedProfiles.Clear();

            foreach (LeaderboardEntry entry in entries)
            {
                SavedProfiles.Add(new UserProfileModel
                {
                    Name = entry.PlayerName,
                    AvatarImage = _leaderboardService.GetAvatarImagePath(entry.AvatarId),
                    HighScore = entry.HighestScore,
                    PlayedDate = entry.AchievedAt == default
                        ? string.Empty
                        : entry.AchievedAt.ToString("MMM d, yyyy")
                });
            }
        }
    }

    public class UserProfileModel
    {
        public string Name { get; set; } = string.Empty;
        public string AvatarImage { get; set; } = string.Empty;
        public int HighScore { get; set; }
        public string PlayedDate { get; set; } = string.Empty;
    }
}
