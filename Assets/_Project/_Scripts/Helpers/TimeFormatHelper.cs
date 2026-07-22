namespace Helpers {
    public static class TimeFormatHelper {
        public static string Format(int totalSeconds) {
            if (totalSeconds <= 0)
                return "0s";

            if (totalSeconds < 60)
                return $"{totalSeconds}s";

            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            if (minutes < 60) {
                if (seconds == 0)
                    return $"{minutes}m";

                return $"{minutes}m {seconds}s";
            }

            int hours = minutes / 60;
            minutes %= 60;

            if (minutes == 0)
                return $"{hours}h";

            return $"{hours}h {minutes}m";
        }
    }
}