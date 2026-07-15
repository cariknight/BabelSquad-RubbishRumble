using Plugin.Maui.Audio;

namespace RubbishRumble.Services
{
    public class SettingsService
    {
        private static SettingsService? _instance;
        public static SettingsService Instance => _instance ??= new SettingsService();

        private IAudioPlayer? _musicPlayer;
        private bool _isMusicMuted;

        public bool IsMusicMuted => _isMusicMuted;
        public bool IsSfxMuted { get; private set; }

        private SettingsService() { }

        public async Task InitializeMusicAsync()
        {
            if (_musicPlayer != null)
                return;

            try
            {
                var audioStream = await FileSystem.OpenAppPackageFileAsync("bgmusic.mp3");
                _musicPlayer = AudioManager.Current.CreatePlayer(audioStream);
                _musicPlayer.Loop = true;
                _musicPlayer.Volume = 0.6;

                if (!_isMusicMuted)
                    _musicPlayer.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize music: {ex.Message}");
            }
        }

        public void ToggleMusic()
        {
            _isMusicMuted = !_isMusicMuted;

            if (_musicPlayer == null)
                return;

            if (_isMusicMuted)
                _musicPlayer.Pause();
            else
                _musicPlayer.Play();
        }

        public void ToggleSfx()
        {
            IsSfxMuted = !IsSfxMuted;
        }

        public async Task PlaySfxAsync(string fileName)
        {
            if (IsSfxMuted)
                return;

            try
            {
                var sfxStream = await FileSystem.OpenAppPackageFileAsync(fileName);
                var sfxPlayer = AudioManager.Current.CreatePlayer(sfxStream);
                var isCleaningUp = false;
                EventHandler? onPlaybackEnded = null;
                onPlaybackEnded = (_, _) =>
                {
                    if (isCleaningUp)
                        return;

                    isCleaningUp = true;
                    sfxPlayer.PlaybackEnded -= onPlaybackEnded;
                    sfxPlayer.Dispose();
                };
                sfxPlayer.PlaybackEnded += onPlaybackEnded;
                sfxPlayer.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to play SFX: {ex.Message}");
            }
        }
    }
}
