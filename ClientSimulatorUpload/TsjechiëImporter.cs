using System;
using System.Collections.Generic;
using System.Text.Json;
using ClientSimulator_DL.Repository;
using ClientSimulatorUtils;
using ClientSimulator_BL.Manager;

namespace ClientSimulatorUpload
{
    public class TsjechiëImporter
    {
        private readonly GemeenteRepository _gemeenteRepo;
        private readonly StraatRepository _straatRepo;
        private readonly GemeenteManager _gemeenteMgr;
        private readonly StraatManager _straatMgr;

        private readonly int _landId;

        public TsjechiëImporter(int landId)
        {
            _landId = landId;

            _gemeenteRepo = new GemeenteRepository();
            _straatRepo = new StraatRepository();
            _gemeenteMgr = new GemeenteManager(_gemeenteRepo);
            _straatMgr = new StraatManager(_straatRepo);
        }

        public void Import()
        {
            Console.WriteLine("=== Tsjechië importeren ===");

            string path = @"C:\Users\hp\Desktop\EINDTAAK\sourceData\Tsjechië\cz.locale.json";

            if (!File.Exists(path))
            {
                Console.WriteLine("❌ Bestand niet gevonden");
                return;
            }

            var json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<CzechLocale>(json);

            if (data?.address?.city_name == null)
            {
                Console.WriteLine("❌ JSON bevat geen geldige city_name data");
                return;
            }

            // Importeer gemeenten
            Console.WriteLine("→ Gemeenten importeren...");
            ImportGemeenten(data);

            // Importeer straten
            Console.WriteLine("→ Straten importeren...");
            ImportStraten(data);

            Console.WriteLine("Tsjechië ✓");
        }

        private void ImportGemeenten(CzechLocale data)
        {
            int success = 0;
            int skipped = 0;
            int failed = 0;

            foreach (var city in data.address.city_name)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(city))
                    {
                        skipped++;
                        continue;
                    }

                    if (_gemeenteMgr.IsOngeldigeGemeente(city))
                    {
                        skipped++;
                        continue;
                    }

                    string clean = Normalizer.Clean(city);
                    _gemeenteRepo.InsertOfOphalen(clean, _landId);
                    success++;
                }
                catch (Exception ex)
                {
                    failed++;
                    Console.WriteLine($"❌ Fout bij gemeente '{city}': {ex.Message}");
                }
            }

            Console.WriteLine($"   Gemeenten - ✔: {success}, ⏭: {skipped}, ❌: {failed}");
        }

        private void ImportStraten(CzechLocale data)
        {
            if (data?.address?.street == null)
            {
                Console.WriteLine("⚠ JSON bevat geen street data");
                return;
            }

            int success = 0;
            int skipped = 0;
            int failed = 0;

            // Voor Tsjechië worden straten willekeurig aan gemeenten gekoppeld
            // We maken één dummy gemeente aan voor alle straten
            int dummyGemeenteId = _gemeenteRepo.InsertOfOphalen("Algemeen", _landId);

            foreach (var street in data.address.street)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(street))
                    {
                        skipped++;
                        continue;
                    }

                    if (_straatMgr.IsOngeldigeStraat(street))
                    {
                        skipped++;
                        continue;
                    }

                    string clean = Normalizer.Clean(street);

                    // Gebruik een standaard wegtype voor Tsjechië
                    string wegtype = "residential";

                    if (!_straatRepo.Exists(dummyGemeenteId, clean))
                    {
                        _straatRepo.Insert(dummyGemeenteId, clean, wegtype);
                        success++;
                    }
                    else
                    {
                        skipped++;
                    }
                }
                catch (Exception ex)
                {
                    failed++;
                    Console.WriteLine($"❌ Fout bij straat '{street}': {ex.Message}");
                }
            }

            Console.WriteLine($"   Straten - ✔: {success}, ⏭: {skipped}, ❌: {failed}");
        }

        // DTO’s
        private class CzechLocale
        {
            public string title { get; set; }
            public Address address { get; set; }
        }

        private class Address
        {
            public List<string> city_name { get; set; }
            public List<string> street { get; set; }
        }
    }
}
