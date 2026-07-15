using System.Text.Json;

namespace RubbishRumble.Services
{
    public class APIService
    {
        private static readonly Random Random = new();
        private static readonly HttpClient HttpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        private static readonly string[] DictionaryWords =
        [
            "recyclable",
            "biodegradable",
            "biohazard",
            "compost",
            "landfill",
            "recycling",
            "plastic",
            "waste"
        ];

        private static readonly string[] WikipediaTopics =
        [
            "Recycling",
            "Compost",
            "Landfill",
            "Plastic_recycling",
            "Waste_management",
            "Biodegradable_waste"
        ];

        public async Task<(string Title, string Text)> GetRandomEcoTipAsync()
        {
            try
            {
                return Random.Next(2) == 0
                    ? await GetRandomDefinitionAsync()
                    : await GetRandomWikipediaSummaryAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to fetch eco tip: {ex}");
                return ("Eco Tip", "Keep sorting trash into the right bins to help the planet!");
            }
        }

        private async Task<(string Title, string Text)> GetRandomDefinitionAsync()
        {
            string word = DictionaryWords[Random.Next(DictionaryWords.Length)];
            string json = await HttpClient.GetStringAsync(
                $"https://api.dictionaryapi.dev/api/v2/entries/en/{word}");

            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement[0];

            string definition = root
                .GetProperty("meanings")[0]
                .GetProperty("definitions")[0]
                .GetProperty("definition")
                .GetString() ?? string.Empty;

            return ($"Word: {char.ToUpper(word[0])}{word[1..]}", Truncate(definition, 220));
        }

        private async Task<(string Title, string Text)> GetRandomWikipediaSummaryAsync()
        {
            string topic = WikipediaTopics[Random.Next(WikipediaTopics.Length)];
            string json = await HttpClient.GetStringAsync(
                $"https://en.wikipedia.org/api/rest_v1/page/summary/{topic}");

            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            string title = root.GetProperty("title").GetString() ?? topic.Replace('_', ' ');
            string extract = root.GetProperty("extract").GetString() ?? string.Empty;

            return (title, Truncate(extract, 220));
        }

        private static string Truncate(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length <= maxLength)
                return text;

            return text[..(maxLength - 3)].TrimEnd() + "...";
        }
    }
}
