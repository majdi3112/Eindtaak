namespace ClientSimulatorUtils
{
    public static class Normalizer
    {
        public static string Clean(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var s = input.Trim();

            s = s.Replace("\"", "")
                 .Replace("'", "")
                 .Replace("(unknown)", "", StringComparison.OrdinalIgnoreCase);

            while (s.Contains("  "))
                s = s.Replace("  ", " ");

            return s.ToUpper();
        }

        public static string CleanName(string name)
        {
            name = Clean(name);
            return name.Replace(".", "").Replace(",", "");
        }
    }
}
