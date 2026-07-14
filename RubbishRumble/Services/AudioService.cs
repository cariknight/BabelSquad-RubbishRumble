namespace RubbishRumble.Services
{
    public class AudioService
    {
        public bool IsMusicEnabled { get; private set; }
        public bool IsSoundEffectsEnabled { get; private set; }

        public void SetMusicEnabled(bool enabled)
        {
            IsMusicEnabled = enabled;

            if (enabled)
            {
                // Start background music when audio assets are available.
            }
            else
            {
                // Stop background music when audio assets are available.
            }
        }

        public void SetSoundEffectsEnabled(bool enabled)
        {
            IsSoundEffectsEnabled = enabled;
        }

        public void PlaySoundEffect(string soundName)
        {
            if (!IsSoundEffectsEnabled)
                return;

            // Play sound effect when audio assets are available.
        }
    }
}
