using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ClientSimulatorUtils
{
    public static class TxtReader
    {
        public static List<string> ReadLines(string path)
        {
            var output = new List<string>();

            if (!File.Exists(path))
            {
                Console.WriteLine($"[TXT] Bestand niet gevonden: {path}");
                return output;
            }

            try
            {
                foreach (var line in File.ReadLines(path, Encoding.UTF8))
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string cleaned = CleanLine(line);
                    if (!string.IsNullOrWhiteSpace(cleaned))
                        output.Add(cleaned);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TXT] Fout bij lezen van {path}: {ex.Message}");
            }

            return output;
        }

        /// <summary>
        /// Parse tab-separated values (TSV) from a text file
        /// </summary>
        public static IEnumerable<string[]> ReadTabSeparated(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"[TSV] Bestand niet gevonden: {path}");
                yield break;
            }

            foreach (var line in File.ReadLines(path, Encoding.UTF8))
            {
                string cleaned = CleanLine(line);
                if (string.IsNullOrWhiteSpace(cleaned))
                    continue;

                // Skip header lines
                if (cleaned.StartsWith("Fornavne") ||
                    cleaned.StartsWith("Efternavne") ||
                    cleaned.StartsWith("Navn") ||
                    cleaned.StartsWith("Tilltalsnamn") ||
                    cleaned.StartsWith("Efternamn") ||
                    cleaned.StartsWith("Etunimi") ||
                    cleaned.StartsWith("Sukunimi") ||
                    cleaned.StartsWith("Frecuencias") ||
                    cleaned.StartsWith("Orden") ||
                    cleaned.StartsWith("First name") ||
                    cleaned.StartsWith("Vorname") ||
                    cleaned.StartsWith("LASTNAME") ||
                    cleaned.StartsWith("Vornamen") ||
                    cleaned.StartsWith("Prénoms") ||
                    cleaned.StartsWith("Nomi") ||
                    cleaned.StartsWith("Nachnamen") ||
                    cleaned.StartsWith("Noms de famille") ||
                    cleaned.StartsWith("Cognomi") ||
                    cleaned.Contains("Edad Media") ||
                    cleaned.Contains("Lukumäärä") ||
                    cleaned.Contains("Medelålder") ||
                    cleaned.Contains("ANTAL") ||
                    cleaned.Contains("januar") ||
                    cleaned.Contains("forekomster") ||
                    cleaned.Contains("flere") ||
                    cleaned.Contains("bärare") ||
                    cleaned.Contains("Bevölkerung") ||
                    cleaned.Contains("population") ||
                    cleaned.Contains("antal") ||
                    cleaned.Contains("Datos procedentes") ||
                    cleaned.Contains("Censos de población") ||
                    cleaned.Contains("frecuencia") ||
                    cleaned.Contains("Apellido") ||
                    cleaned.Contains("frekvens") ||
                    cleaned.Contains("apellidos") ||
                    cleaned.Contains("weiblich") ||
                    cleaned.Contains("männlich") ||
                    cleaned.Contains("femminin") ||
                    cleaned.Contains("masculin") ||
                    cleaned.Contains("femminile") ||
                    cleaned.Contains("maschile") ||
                    cleaned.Contains("female") ||
                    cleaned.Contains("male"))
                    continue;

                // Split on tabs
                var parts = cleaned.Split('\t', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                    yield return parts;
            }
        }

        private static string CleanLine(string line)
        {
            // verwijder BOM + unicode junk
            string s = line.Trim('\uFEFF', '\u200B').Trim();

            // verwijder control characters
            var sb = new StringBuilder(s.Length);
            foreach (char c in s)
            {
                if (!char.IsControl(c))
                    sb.Append(c);
            }

            return sb.ToString().Trim();
        }

        // ------------------------------------------------------------
        // SPACE-SEPARATED READER (for Spanish data)
        // ------------------------------------------------------------
        public static IEnumerable<string[]> ReadSpaceSeparated(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"[SPACE] Bestand niet gevonden: {path}");
                yield break;
            }

            foreach (var line in File.ReadLines(path, Encoding.UTF8))
            {
                string cleaned = CleanLine(line);
                if (string.IsNullOrWhiteSpace(cleaned))
                    continue;

                // Skip header lines
                if (cleaned.StartsWith("Frecuencias") ||
                    cleaned.StartsWith("Datos") ||
                    cleaned.StartsWith("Nombres") ||
                    cleaned.StartsWith("Orden") ||
                    cleaned.StartsWith("Apellidos") ||
                    cleaned.Contains("Edad Media") ||
                    cleaned.Contains("frecuencia") ||
                    cleaned.Contains("apellido") ||
                    cleaned.Contains("Censos"))
                    continue;

                // Split on multiple spaces (Spanish data uses variable spaces)
                var parts = System.Text.RegularExpressions.Regex.Split(cleaned.Trim(), @"\s{2,}")
                           .Where(p => !string.IsNullOrWhiteSpace(p))
                           .ToArray();

                if (parts.Length > 0)
                    yield return parts;
            }
        }
    }
}
