using System;
using System.IO;
using System.Text.Json;

namespace ClientSimulatorUtils
{
    public static class JsonReader
    {
        public static T Load<T>(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    Console.WriteLine($"[JSON] Bestand niet gevonden: {path}");
                    return default;
                }

                string json = File.ReadAllText(path);

                if (string.IsNullOrWhiteSpace(json))
                {
                    Console.WriteLine($"[JSON] Leeg bestand: {path}");
                    return default;
                }

                return JsonSerializer.Deserialize<T>(json);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"[JSON] Fout tijdens JSON-parsen: {ex.Message}");
                return default;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JSON] Onbekende fout: {ex.Message}");
                return default;
            }
        }
    }
}
