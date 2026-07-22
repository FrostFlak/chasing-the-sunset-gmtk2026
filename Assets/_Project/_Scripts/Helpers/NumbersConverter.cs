namespace Helpers {
    public static class NumbersConverter {
        public static string ToShortHand(long number) {
            if (number >= 1000000000) return (number / 1000000000) + "B";
            if (number >= 1000000) return (number / 1000000) + "M";
            if (number >= 1000) return (number / 1000) + "K";
            return number.ToString();
        }

        public static string ToRoman(int number) {
            if (number <= 0) 
                return "";

            var romanNumerals = new (int Value, string Symbol)[] {
                (1000, "M"), (900, "CM"), (500, "D"), (400, "CD"),
                (100, "C"), (90, "XC"), (50, "L"), (40, "XL"),
                (10, "X"), (9, "IX"), (5, "V"), (4, "IV"), (1, "I")
            };

            var result = new System.Text.StringBuilder();
            foreach (var (value, symbol) in romanNumerals) {
                while (number >= value) {
                    result.Append(symbol);
                    number -= value;
                }
            }

            return result.ToString();
        }
    }
}