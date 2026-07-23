namespace RubbishRumble.Models
{
    public class TrashItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Rarity { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;

        public string GetMauiImageSource()
        {
            if (string.IsNullOrWhiteSpace(FilePath))
                return string.Empty;

            string normalized = FilePath.Replace('\\', '/');
            return Path.GetFileName(normalized);
        }
    }
}
