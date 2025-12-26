using System;
using System.Collections.Generic;
using System.IO;

namespace ClientSimulatorUtils
{
    public static class CsvReader
    {
        public static IEnumerable<string[]> Read(string path)
        {
            if (!File.Exists(path))
                yield break;

            using var reader = new StreamReader(path);

            string? header = reader.ReadLine();
            if (header == null)
                yield break;

            char delimiter = DetectDelimiter(header);

            while (!reader.EndOfStream)
            {
                string? line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                line = line.Trim('"');

                var rawParts = line.Split(delimiter, StringSplitOptions.None);

                var parts = new string[rawParts.Length];
                for (int i = 0; i < rawParts.Length; i++)
                    parts[i] = rawParts[i].Trim();

                yield return parts;
            }
        }

        private static char DetectDelimiter(string line)
        {
            if (line.Contains(";")) return ';';
            if (line.Contains(",")) return ',';
            if (line.Contains("\t")) return '\t';
            return ';';
        }
    }
}
