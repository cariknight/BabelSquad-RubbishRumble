using RubbishRumble.Models;

namespace RubbishRumble.Services
{
    public class SettingsService
    {
        private readonly DatabaseService _database;
        private readonly AudioService _audioService;
        private Settings? _currentSettings;

        public SettingsService(DatabaseService database, AudioService audioService)
        {
            _database = database;
            _audioService = audioService;
        }

        public Settings CurrentSettings => _currentSettings ?? new Settings();

        public async Task<Settings> LoadSettingsAsync()
        {
            _currentSettings = await _database.GetSettingsAsync();
            ApplySettings(_currentSettings);
            return _currentSettings;
        }

        public async Task<Settings> GetSettingsAsync()
        {
            if (_currentSettings == null)
                return await LoadSettingsAsync();

            return _currentSettings;
        }

        public async Task UpdateSettingsAsync(Settings settings)
        {
            Settings current = await GetSettingsAsync();

            current.MusicEnabled = settings.MusicEnabled;
            current.SoundEffectsEnabled = settings.SoundEffectsEnabled;
            current.TutorialCompleted = settings.TutorialCompleted;
            current.TotalCoins = settings.TotalCoins;

            await SaveAndApplyAsync(current);
        }

        public async Task SetMusicEnabledAsync(bool enabled)
        {
            Settings settings = await GetSettingsAsync();
            settings.MusicEnabled = enabled;
            await SaveAndApplyAsync(settings);
        }

        public async Task SetSoundEffectsEnabledAsync(bool enabled)
        {
            Settings settings = await GetSettingsAsync();
            settings.SoundEffectsEnabled = enabled;
            await SaveAndApplyAsync(settings);
        }

        public async Task SetTutorialCompletedAsync(bool completed)
        {
            Settings settings = await GetSettingsAsync();
            settings.TutorialCompleted = completed;
            await SaveAndApplyAsync(settings);
        }

        public async Task SetTotalCoinsAsync(int totalCoins)
        {
            Settings settings = await GetSettingsAsync();
            settings.TotalCoins = totalCoins;
            await SaveAndApplyAsync(settings);
        }

        public async Task SaveSettingsAsync()
        {
            if (_currentSettings == null)
                return;

            await _database.SaveSettingsAsync(_currentSettings);
            ApplySettings(_currentSettings);
        }

        public void ApplySettings(Settings settings)
        {
            _audioService.SetMusicEnabled(settings.MusicEnabled);
            _audioService.SetSoundEffectsEnabled(settings.SoundEffectsEnabled);
        }

        private async Task SaveAndApplyAsync(Settings settings)
        {
            await _database.SaveSettingsAsync(settings);
            _currentSettings = settings;
            ApplySettings(settings);
        }
    }
}
