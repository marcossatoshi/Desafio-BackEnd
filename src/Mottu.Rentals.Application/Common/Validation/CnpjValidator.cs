namespace Mottu.Rentals.Application.Common.Validation;

public static class CnpjValidator
{
    public static string Normalize(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj)) return string.Empty;
        var digits = new char[cnpj.Length];
        var idx = 0;
        foreach (var ch in cnpj)
        {
            if (char.IsDigit(ch)) digits[idx++] = ch;
        }
        return new string(digits, 0, idx);
    }

    public static bool IsValid(string? cnpj)
    {
        var value = Normalize(cnpj ?? string.Empty);
        if (value.Length != 14) return false;
        if (IsRepeatedSequence(value)) return false;

        var base12 = value.Substring(0, 12);
        var d1 = CalculateCheckDigit(base12, new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 });
        var d2 = CalculateCheckDigit(base12 + d1, new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 });

        return value.EndsWith($"{d1}{d2}");
    }

    private static bool IsRepeatedSequence(string value)
    {
        for (var i = 1; i < value.Length; i++)
        {
            if (value[i] != value[0]) return false;
        }
        return true;
    }

    private static int CalculateCheckDigit(string input, int[] weights)
    {
        var sum = 0;
        for (var i = 0; i < weights.Length; i++)
        {
            sum += (input[i] - '0') * weights[i];
        }
        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }
}


