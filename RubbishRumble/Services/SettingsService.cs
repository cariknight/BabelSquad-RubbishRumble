using Plugin.Maui.Audio;

namespace RubbishRumble.Services
{
    public class SettingsService
    {
        private static SettingsService? _instance;
        public static SettingsService Instance => _instance ??= new SettingsService();

        private IAudioPlayer? _musicPlayer;
        private bool _isMusicMuted;
        private bool _isAppActive = true;
        private bool _musicPausedForInactivity;

        public bool IsMusicMuted => _isMusicMuted;
        public bool IsSfxMuted { get; private set; }
        public bool IsAppActive => _isAppActive;

        public event EventHandler? AppBecameInactive;
        public event EventHandler? AppBecameActive;

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

                if (!_isMusicMuted && _isAppActive)
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
            else if (_isAppActive)
                _musicPlayer.Play();
        }

        public void PauseForAppInactive()
        {
            if (!_isAppActive)
                return;

            _isAppActive = false;

            if (_musicPlayer == null || _isMusicMuted)
            {
                AppBecameInactive?.Invoke(this, EventArgs.Empty);
                return;
            }

            _musicPlayer.Pause();
            _musicPausedForInactivity = true;
            AppBecameInactive?.Invoke(this, EventArgs.Empty);
        }

        public void ResumeFromAppActive()
        {
            if (_isAppActive)
                return;

            _isAppActive = true;

            if (_musicPlayer == null || _isMusicMuted || !_musicPausedForInactivity)
            {
                AppBecameActive?.Invoke(this, EventArgs.Empty);
                return;
            }

            _musicPlayer.Play();
            _musicPausedForInactivity = false;
            AppBecameActive?.Invoke(this, EventArgs.Empty);
        }

        public void ToggleSfx()
        {
            IsSfxMuted = !IsSfxMuted;
        }

        public async Task PlaySfxAsync(string fileName)
        {
            if (IsSfxMuted || !_isAppActive)
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
