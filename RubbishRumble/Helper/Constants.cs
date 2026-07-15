namespace RubbishRumble.Helper
{
    public class Constants
    {
        public const int STARTING_LIVES = 3;

        // Spawn Settings
        public const double STARTING_SPAWN_INTERVAL = 3.0;
        public const double MIN_SPAWN_INTERVAL = 0.8;

        // Trash Speed settings
        public const double STARTING_TRASH_SPEED = 1.0;
        public const double MAX_TRASH_SPEED = 5.0;

        // Difficulty settings
        public const int DIFFICULTY_INCREASE_EVERY = 20;

        public const double SPAWN_INTERVAL_DECREASE = 0.1;
        public const double TRASH_SPEED_INCREASE = 0.15;

        // Sorting
        public const double BIN_ZONE_START_RATIO = 0.65;

        // Economy: 10 score = 1 coin (Speed costs 10, Freeze/Slow 15, Auto Sort 50)
        public const int SCORE_TO_COINS_DIVISOR = 10;
        public const int MIN_COINS_EARNED = 1;

        // Bonus when beating personal best: 1.5x coins (e.g. 300 score -> 30 base -> 45 coins)
        public const double HIGH_SCORE_COIN_MULTIPLIER = 1.5;
    }
}
