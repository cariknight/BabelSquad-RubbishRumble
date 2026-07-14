using RubbishRumble.Models;
using System.Text.Json;

namespace RubbishRumble.Services
{
    public class PowerUpService
    {
        private readonly Dictionary<string, DateTime> _activePowerUps = new();
        public List<PowerUp> PowerUps { get; private set; } = new();

        //Read powerup.json

        public async Task InitializeAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("powerup.json");
            using StreamReader reader = new(stream);

            string json = await reader.ReadToEndAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            PowerUps = JsonSerializer.Deserialize<List<PowerUp>>(json, options) ?? new List<PowerUp>();
        }

        public bool IsActive(string effectType)
        {
            return _activePowerUps.ContainsKey(effectType);
        }

        public async Task ActivateAsync(string effectType)
        {
            PowerUp? powerUp = PowerUps.FirstOrDefault(
                p => p.EffectType == effectType);

            if (powerUp == null)
                return;

            _activePowerUps[effectType] = DateTime.Now.AddSeconds(powerUp.DurationSeconds);
            await Task.Delay(TimeSpan.FromSeconds(powerUp.DurationSeconds));

            _activePowerUps.Remove(effectType);
        }

        public PowerUp? GetPowerUp(string effectType)
        {
            return PowerUps.FirstOrDefault(p => p.EffectType == effectType);
        }
    }
}
