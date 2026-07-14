namespace RubbishRumble.Models
{
    public class TrashItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Rarity { get; set; }
        public string FilePath { get; set; }

        public string GetMauiImageSource()
        {
            if (string.IsNullOrWhiteSpace(FilePath))
                return string.Empty;

            const string prefix = "Resources/Images/";
            string path = FilePath.Replace('\\', '/');

            if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                path = path[prefix.Length..];

            return Path.ChangeExtension(path, null);
        }
    }
}
