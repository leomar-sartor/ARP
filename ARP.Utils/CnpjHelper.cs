namespace ARP.Utils
{
    public static class CnpjHelper
    {
        private static readonly int[] Weights1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        private static readonly int[] Weights2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        public static bool IsValidCnpj(string? cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj))
                return false;

            Span<char> chars = stackalloc char[14];
            int length = 0;

            foreach (char c in cnpj)
            {
                if (!char.IsLetterOrDigit(c))
                    continue;

                if (length >= 14)
                    return false;

                chars[length++] = char.ToUpperInvariant(c);
            }

            if (length != 14)
                return false;

            if (!char.IsDigit(chars[12]) || !char.IsDigit(chars[13]))
                return false;

            int dv1 = CalculateDigit(chars[..12], Weights1);

            if (dv1 != chars[12] - '0')
                return false;

            Span<char> first13 = stackalloc char[13];

            for (int i = 0; i < 13; i++)
                first13[i] = chars[i];

            int dv2 = CalculateDigit(first13, Weights2);

            return dv2 == chars[13] - '0';
        }

        private static int CalculateDigit(ReadOnlySpan<char> values, ReadOnlySpan<int> weights)
        {
            int sum = 0;

            for (int i = 0; i < values.Length; i++)
            {
                sum += GetValue(values[i]) * weights[i];
            }

            int remainder = sum % 11;

            return remainder < 2
                ? 0
                : 11 - remainder;
        }

        private static int GetValue(char c)
        {
            return c - '0';
        }

        public static string OnlyLettersAndDigits(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            int count = 0;

            foreach (char c in value)
            {
                if (char.IsLetterOrDigit(c))
                    count++;
            }

            return string.Create(count, value, (span, source) =>
            {
                int index = 0;

                foreach (char c in source)
                {
                    if (char.IsLetterOrDigit(c))
                        span[index++] = char.ToUpperInvariant(c);
                }
            });
        }
    }
}
