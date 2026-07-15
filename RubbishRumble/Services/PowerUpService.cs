using RubbishRumble.Models;
using System.Text.Json;

namespace RubbishRumble.Services
{
    public class PowerUpService
    {
        public List<PowerUp> PowerUps { get; private set; } = new();

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
    }
}
