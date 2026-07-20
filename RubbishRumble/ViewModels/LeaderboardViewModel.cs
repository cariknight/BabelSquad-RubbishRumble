using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using RubbishRumble.Models;
using RubbishRumble.Services;

namespace RubbishRumble.ViewModels
{
    public class LeaderboardViewModel : BaseViewModel
    {
        private string _playerNameText = string.Empty;
        public string PlayerNameText
        {
            get => _playerNameText;
            set => SetProperty(ref _playerNameText, value);
        }

        private string _selectedAvatarImage = "biodegradable_01.png";
        public string SelectedAvatarImage
        {
            get => _selectedAvatarImage;
            set => SetProperty(ref _selectedAvatarImage, value);
        }

        public ObservableCollection<string> TrashAvatars { get; } = new();
        public ObservableCollection<UserProfileModel> SavedProfiles { get; } = new();

        public ICommand AddProfileCommand { get; }
        public ICommand DeleteProfileCommand { get; }

        public LeaderboardViewModel()
        {
            AddProfileCommand = new Command(OnAddProfile);
            DeleteProfileCommand = new Command<UserProfileModel>(OnDeleteProfile);
            LoadAvatarOptions();
            LoadMockProfiles();
        }

        private void LoadAvatarOptions()
        {
            TrashAvatars.Add("biodegradable_01.png");
            TrashAvatars.Add("recyclables_08.png");
            TrashAvatars.Add("biohazard_09.png");
            TrashAvatars.Add("landfall_06.png");

            // Default profile
            SelectedAvatarImage = TrashAvatars.FirstOrDefault() ?? "biodegradable_01.png";
        }

        private void OnAddProfile()
        {
            if (string.IsNullOrWhiteSpace(PlayerNameText))
                return;

            var newProfile = new UserProfileModel
            {
                Name = PlayerNameText.Trim(),
                AvatarImage = SelectedAvatarImage,
                HighScore = 0
            };

            SavedProfiles.Add(newProfile);
            PlayerNameText = string.Empty;
        }

        private void OnDeleteProfile(UserProfileModel profile)
        {
            if (profile != null && SavedProfiles.Contains(profile))
            {
                SavedProfiles.Remove(profile);
            }
        }

        // Placeholder for high scores
        private void LoadMockProfiles()
        {
            SavedProfiles.Add(new UserProfileModel { Name = "Rubbish Rumble", AvatarImage = "biodegradable_01.png", HighScore = 1250 });
        }
    }

    public class UserProfileModel
    {
        public string Name { get; set; } = string.Empty;
        public string AvatarImage { get; set; } = string.Empty;
        public int HighScore { get; set; }
    }
}
