using System.Collections.ObjectModel;
using System.Windows.Input;
using RubbishRumble.Models;
using RubbishRumble.Services;

namespace RubbishRumble.ViewModels
{
    public class LeaderboardViewModel : BaseViewModel
    {
        private readonly LeaderboardService _leaderboardService = new();

        private string _playerNameText = string.Empty;
        private AvatarOption? _selectedAvatar;
        private int _pendingScore;
        private string _submitErrorMessage = string.Empty;

        public string PlayerNameText
        {
            get => _playerNameText;
            set => SetProperty(ref _playerNameText, value);
        }

        public AvatarOption? SelectedAvatar
        {
            get => _selectedAvatar;
            set
            {
                if (SetProperty(ref _selectedAvatar, value))
                    OnPropertyChanged(nameof(SelectedAvatarImage));
            }
        }

        public string SelectedAvatarImage => SelectedAvatar?.ImagePath ?? string.Empty;

        public bool CanSubmitEntry
        {
            get;
            private set;
        }

        public string SubmitErrorMessage
        {
            get => _submitErrorMessage;
            private set => SetProperty(ref _submitErrorMessage, value);
        }

        public ObservableCollection<AvatarOption> Avatars { get; } = new();
        public ObservableCollection<UserProfileModel> SavedProfiles { get; } = new();

        public ICommand AddProfileCommand { get; }

        public LeaderboardViewModel()
        {
            AddProfileCommand = new Command(async () => await OnAddProfileAsync(), () => CanSubmitEntry);
        }

        public async Task InitializeAsync()
        {
            LoadAvatarOptions();
            await LoadLeaderboardAsync();
            await RefreshSubmitStateAsync();
        }

        private void LoadAvatarOptions()
        {
            Avatars.Clear();

            foreach (AvatarOption avatar in _leaderboardService.GetAvailableAvatars())
                Avatars.Add(avatar);

            SelectedAvatar = Avatars.FirstOrDefault();
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
                    HighScore = entry.HighestScore
                });
            }
        }

        private async Task RefreshSubmitStateAsync()
        {
            _pendingScore = await _leaderboardService.GetPendingLeaderboardScoreAsync();
            CanSubmitEntry = await _leaderboardService.CanSubmitLeaderboardEntryAsync(_pendingScore);

            if (!CanSubmitEntry)
                SubmitErrorMessage = string.Empty;

            OnPropertyChanged(nameof(CanSubmitEntry));
            (AddProfileCommand as Command)?.ChangeCanExecute();
        }

        private async Task OnAddProfileAsync()
        {
            SubmitErrorMessage = string.Empty;

            if (!CanSubmitEntry || SelectedAvatar == null)
                return;

            LeaderboardSubmitResult result = await _leaderboardService.SubmitLeaderboardEntryAsync(
                PlayerNameText,
                SelectedAvatar.Id,
                _pendingScore);

            if (!result.Success)
            {
                SubmitErrorMessage = result.ErrorMessage;
                return;
            }

            PlayerNameText = string.Empty;
            await LoadLeaderboardAsync();
            await RefreshSubmitStateAsync();
        }
    }

    public class UserProfileModel
    {
        public string Name { get; set; } = string.Empty;
        public string AvatarImage { get; set; } = string.Empty;
        public int HighScore { get; set; }
    }
}
