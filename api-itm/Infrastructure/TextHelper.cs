using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_itm.Infrastructure
{
    public static class TextHelper
    {
        // Converts ANY value (int, string, null) to a safe string.
        public static string ToSafeString(object? value)
            => value?.ToString() ?? string.Empty;

        public static string RemoveDiacritics(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            var norm = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(norm.Length);
            foreach (var ch in norm)
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        // Accepts ANY type, not just string. Safely uppercases and strips diacritics.
        public static string NormalizeNationality(object? value)
        {
            var x = RemoveDiacritics(ToSafeString(value)).Trim().ToUpperInvariant();
            if (x.StartsWith("ROM")) return "ROMANA";
            if (x.StartsWith("UNG")) return "UNGARA";
            if (x.StartsWith("GERM")) return "GERMANA";
            if (x.StartsWith("FRAN")) return "FRANCEZA";
            if (x.StartsWith("ITAL")) return "ITALIANA";
            return x; // fallback: already normalized uppercase
        }

        public static string NormalizeCountry(object? value)
        {
            var x = RemoveDiacritics(ToSafeString(value)).Trim().ToUpperInvariant();
            if (x.StartsWith("ROM")) return "ROMANIA";
            if (x.StartsWith("MOLD")) return "MOLDOVA (REPUBLICA)";
            if (x.StartsWith("GERM")) return "GERMANIA";
            if (x.StartsWith("UNG")) return "UNGARIA";
            if (x.StartsWith("FRAN")) return "FRANTA";
            return x;
        }
    }
}