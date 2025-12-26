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
        private readonly VoornaamRepository _voornaamRepo;
        private readonly AchternaamRepository _achternaamRepo;
        private readonly GemeenteRepository _gemeenteRepo;
        private readonly StraatRepository _straatRepo;

        private readonly VoornaamManager _voornaamMgr;
        private readonly AchternaamManager _achternaamMgr;
        private readonly GemeenteManager _gemeenteMgr;
        private readonly StraatManager _straatMgr;

        private readonly int _landId;

        public TsjechiëImporter(int landId)
        {
            _landId = landId;

            _voornaamRepo = new VoornaamRepository();
            _achternaamRepo = new AchternaamRepository();
            _gemeenteRepo = new GemeenteRepository();
            _straatRepo = new StraatRepository();

            _voornaamMgr = new VoornaamManager(_voornaamRepo);
            _achternaamMgr = new AchternaamManager(_achternaamRepo);
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

            // Importeer voornamen
            Console.WriteLine("→ Voornamen importeren...");
            ImportVoornamen(data);

            // Importeer achternamen
            Console.WriteLine("→ Achternamen importeren...");
            ImportAchternamen(data);

            // Importeer gemeenten
            Console.WriteLine("→ Gemeenten importeren...");
            ImportGemeenten(data);

            // Importeer straten
            Console.WriteLine("→ Straten importeren...");
            ImportStraten(data);

            Console.WriteLine("Tsjechië ✓");
        }

        private void ImportVoornamen(CzechLocale data)
        {
            int success = 0;
            int skipped = 0;
            int failed = 0;

            // Mannennamen
            if (data.name?.male_first_name != null)
            {
                foreach (var raw in data.name.male_first_name)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(raw))
                        {
                            skipped++;
                            continue;
                        }

                        string naam = Normalizer.Clean(raw);
                        if (_voornaamRepo.Exists(naam, "M", _landId))
                        {
                            skipped++;
                            continue;
                        }

                        _voornaamMgr.ValideerVoornaam(naam);
                        _voornaamRepo.Insert(naam, "M", 1, _landId);
                        success++;
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        Console.WriteLine($"❌ Fout bij voornaam '{raw}': {ex.Message}");
                    }
                }
            }

            // Vrouwennamen
            if (data.name?.female_first_name != null)
            {
                foreach (var raw in data.name.female_first_name)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(raw))
                        {
                            skipped++;
                            continue;
                        }

                        string naam = Normalizer.Clean(raw);
                        if (_voornaamRepo.Exists(naam, "F", _landId))
                        {
                            skipped++;
                            continue;
                        }

                        _voornaamMgr.ValideerVoornaam(naam);
                        _voornaamRepo.Insert(naam, "F", 1, _landId);
                        success++;
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        Console.WriteLine($"❌ Fout bij voornaam '{raw}': {ex.Message}");
                    }
                }
            }

            Console.WriteLine($"   Voornamen - ✔: {success}, ⏭: {skipped}, ❌: {failed}");
        }

        private void ImportAchternamen(CzechLocale data)
        {
            int success = 0;
            int skipped = 0;
            int failed = 0;

            // Mannen achternamen
            if (data.name?.male_last_name != null)
            {
                foreach (var raw in data.name.male_last_name)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(raw))
                        {
                            skipped++;
                            continue;
                        }

                        string naam = Normalizer.Clean(raw);
                        if (_achternaamRepo.Exists(naam, _landId))
                        {
                            skipped++;
                            continue;
                        }

                        _achternaamMgr.ValideerAchternaam(naam);
                        _achternaamRepo.Insert(naam, 1, _landId);
                        success++;
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        Console.WriteLine($"❌ Fout bij achternaam '{raw}': {ex.Message}");
                    }
                }
            }

            // Vrouwen achternamen
            if (data.name?.female_last_name != null)
            {
                foreach (var raw in data.name.female_last_name)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(raw))
                        {
                            skipped++;
                            continue;
                        }

                        string naam = Normalizer.Clean(raw);
                        if (_achternaamRepo.Exists(naam, _landId))
                        {
                            skipped++;
                            continue;
                        }

                        _achternaamMgr.ValideerAchternaam(naam);
                        _achternaamRepo.Insert(naam, 1, _landId);
                        success++;
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        Console.WriteLine($"❌ Fout bij achternaam '{raw}': {ex.Message}");
                    }
                }
            }

            Console.WriteLine($"   Achternamen - ✔: {success}, ⏭: {skipped}, ❌: {failed}");
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

        // DTO's
        private class CzechLocale
        {
            public string title { get; set; }
            public Address address { get; set; }
            public Names name { get; set; }
        }

        private class Address
        {
            public List<string> city_name { get; set; }
            public List<string> street { get; set; }
        }

        private class Names
        {
            public List<string> male_first_name { get; set; }
            public List<string> female_first_name { get; set; }
            public List<string> male_last_name { get; set; }
            public List<string> female_last_name { get; set; }
        }
    }
}
