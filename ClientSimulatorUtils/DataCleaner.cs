using System;
using System.Text;
using System.Text.RegularExpressions;

namespace ClientSimulatorUtils
{
    public static class DataCleaner
    {
        public static string MaakSchoon(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Unicode normalisatie (belangrijk voor Tsjechië!)
            string s = input.Normalize(NormalizationForm.FormC);

            // Trim standaard spaties, tabs en newline-rotzooi
            s = s.Trim();

            // Verwijder control characters (unicode ranges)
            s = RemoveControlCharacters(s);

            // Verwijder foute markeringen
            s = RemoveGarbageMarkers(s);

            // Verwijder overbodige quotes of apostrophes
            s = s.Replace("\"", "")
                 .Replace("'", "");

            // Multiple spaces, tabs, etc. → enkele spatie
            s = Regex.Replace(s, @"\s+", " ");

            return s.Trim();
        }

        private static string RemoveControlCharacters(string s)
        {
            var builder = new StringBuilder(s.Length);

            foreach (char c in s)
            {
                if (!char.IsControl(c))
                    builder.Append(c);
            }

            return builder.ToString();
        }

        private static string RemoveGarbageMarkers(string s)
        {
            string[] garbage =
            {
                "(unknown)", "unknown", "UNKNOWN",
                "(null)", "null", "NULL",
                "n/a", "N/A",
                "<unknown>", "[unknown]"
            };

            foreach (var marker in garbage)
            {
                s = s.Replace(marker, "", StringComparison.OrdinalIgnoreCase);
            }

            return s;
        }
    }
}
