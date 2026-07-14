namespace RubbishRumble.Helper
{
    public static class EconomyHelper
    {
        public static int CalculateEarnedCoins(int score, bool isNewHighScore)
        {
            if (score <= 0)
                return 0;

            int baseCoins = Math.Max(
                Constants.MIN_COINS_EARNED,
                score / Constants.SCORE_TO_COINS_DIVISOR);

            if (!isNewHighScore)
                return baseCoins;

            return Math.Max(
                Constants.MIN_COINS_EARNED,
                (int)Math.Round(
                    baseCoins * Constants.HIGH_SCORE_COIN_MULTIPLIER,
                    MidpointRounding.AwayFromZero));
        }
    }
}
