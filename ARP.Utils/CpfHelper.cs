namespace ARP.Utils
{
    public static class CpfHelper
    {
        public static bool IsValidCpf(string? cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            Span<int> digits = stackalloc int[11];
            int index = 0;

            foreach (char c in cpf)
            {
                if (!char.IsDigit(c))
                    continue;

                if (index >= 11)
                    return false;

                digits[index++] = c - '0';
            }

            if (index != 11)
                return false;

            // Evita sequências repetidas (11111111111, 22222222222, etc.)
            bool allEqual = true;
            for (int i = 1; i < 11; i++)
            {
                if (digits[i] != digits[0])
                {
                    allEqual = false;
                    break;
                }
            }

            if (allEqual)
                return false;

            // Primeiro dígito verificador
            int sum = 0;
            for (int i = 0; i < 9; i++)
                sum += digits[i] * (10 - i);

            int remainder = sum % 11;
            int digit1 = remainder < 2 ? 0 : 11 - remainder;

            if (digits[9] != digit1)
                return false;

            // Segundo dígito verificador
            sum = 0;
            for (int i = 0; i < 10; i++)
                sum += digits[i] * (11 - i);

            remainder = sum % 11;
            int digit2 = remainder < 2 ? 0 : 11 - remainder;

            return digits[10] == digit2;
        }

        public static string OnlyDigits(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            int digitCount = 0;

            foreach (char c in value)
            {
                if (char.IsDigit(c))
                    digitCount++;
            }

            return string.Create(digitCount, value, (span, source) =>
            {
                int index = 0;

                foreach (char c in source)
                {
                    if (char.IsDigit(c))
                        span[index++] = c;
                }
            });
        }
    }
}
