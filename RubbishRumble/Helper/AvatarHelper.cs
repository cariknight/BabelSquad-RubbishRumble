using RubbishRumble.Models;

namespace RubbishRumble.Helper
{
    public static class AvatarHelper
    {
        private static readonly AvatarOption[] Avatars =
        {
            new() { Id = "recyclables_01", ImagePath = "recyclables_01.png" },
            new() { Id = "recyclables_02", ImagePath = "recyclables_02.png" },
            new() { Id = "biodegradable_01", ImagePath = "biodegradable_01.png" },
            new() { Id = "biodegradable_02", ImagePath = "biodegradable_02.png" },
            new() { Id = "biohazard_01", ImagePath = "biohazard_01.png" },
            new() { Id = "biohazard_02", ImagePath = "biohazard_02.png" },
            new() { Id = "landfall_01", ImagePath = "landfall_01.png" },
            new() { Id = "landfall_02", ImagePath = "landfall_02.png" },
        };

        public static IReadOnlyList<AvatarOption> GetAvailableAvatars() => Avatars;

        public static bool IsValidAvatarId(string avatarId) =>
            Avatars.Any(a => a.Id.Equals(avatarId, StringComparison.OrdinalIgnoreCase));

        public static string GetAvatarImagePath(string avatarId)
        {
            AvatarOption? avatar = Avatars.FirstOrDefault(
                a => a.Id.Equals(avatarId, StringComparison.OrdinalIgnoreCase));

            return avatar?.ImagePath ?? string.Empty;
        }
    }
}
