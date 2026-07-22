namespace Helpers.ExtMethods {
    public static class RandomHelper {

        public static float Range(this System.Random random, float min, float max) {
            return (float)(random.NextDouble() * (max - min) + min);
        }

        public static int Range(this System.Random random, int min, int max) {
            return random.Next(min, max);
        }
    }
}