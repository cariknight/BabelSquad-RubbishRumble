using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RubbishRumble.Models;
using System.Text.Json;

namespace RubbishRumble.Services
{
    public class PowerUpService
    {
        private readonly Dictionary<string, DateTime> _activePowerUps = new();

        private readonly List<PowerUp> _powerUps;
        public List<PowerUp> PowerUps { get; private set; } = new();

        //Read powerup.json

        public async Task InitializeAsync()
        {
            using Stream stream = await FileSystem.OpenAppPackageFileAsync("powerup.json");
            using StreamReader reader = new(stream);

            string json = await reader.ReadToEndAsync();
            PowerUps = JsonSerializer.Deserialize<List<PowerUp>>(json) ?? new List<PowerUp>();
        }

        public PowerUpService(List<PowerUp> powerUps)
        {
            _powerUps = powerUps;
        }

        public bool IsActive(string effectType)
        {
            return _activePowerUps.ContainsKey(effectType);
        }

        public async Task ActivateASync(string effectType)
        {
            PowerUp? powerUp = _powerUps.FirstOrDefault(
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
