using Plugin.Maui.Audio;
using RubbishRumble.Services;
using RubbishRumble.ViewModels;

namespace RubbishRumble.Services
{
    public class SettingsService
    {
        private static SettingsService _instance;
        public static SettingsService Instance => _instance ??= new SettingsService();

        private IAudioPlayer _musicPlayer;
        private bool _isMusicMuted = false;
        private bool _isSfxMuted = false;

        private double _musicVolume = 0.6; // 80%
        private double _sfxVolume = 1.0; // 100%
        public bool IsMusicMuted => _isMusicMuted;
        public bool IsSfxMuted => _isSfxMuted;

        private SettingsService() { }
        public async Task InitializeMusicAsync()
        {
            if (_musicPlayer == null)
            {
                try
                {
                    // Resources/Raw folder
                    var audioStream = await FileSystem.OpenAppPackageFileAsync("bgmusic.mp3");
                    _musicPlayer = AudioManager.Current.CreatePlayer(audioStream);
                    _musicPlayer.Loop = true; 

                    if (!_isMusicMuted)
                    {
                        _musicPlayer.Play();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to initialize music: {ex.Message}");
                }
            }
        }
        public void ToggleMusic()
        {
            _isMusicMuted = !_isMusicMuted;

            if (_musicPlayer == null) 
                return;

            if (_isMusicMuted)
            {
                _musicPlayer.Pause();
            }
            else
            {
                _musicPlayer.Play();
            }
        }
        public void ToggleSfx()
        {
            _isSfxMuted = !_isSfxMuted;
        }

        public async Task PlaySfxAsync(string fileName)
        {
            if (_isSfxMuted) return;

            try
            {
                var sfxStream = await FileSystem.OpenAppPackageFileAsync(fileName);
                var sfxPlayer = AudioManager.Current.CreatePlayer(sfxStream);
                sfxPlayer.Play();
                sfxPlayer.PlaybackEnded += (s, e) => sfxPlayer.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to play SFX: {ex.Message}");
            }
        }
    }
}
