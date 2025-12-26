using System;
using System.Collections.Generic;
using System.Text.Json;
using ClientSimulator_DL.Repository;
using ClientSimulatorUtils;
using ClientSimulator_BL.Manager;

namespace ClientSimulatorUpload
{
    public class PolandImporter
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

        public PolandImporter(int landId)
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
            Console.WriteLine("=== Polen importeren ===");

            string jsonPath = @"C:\Users\hp\Desktop\EINDTAAK\sourceData\Polen\pl.locale.json";
            string streetsPath = @"C:\Users\hp\Desktop\EINDTAAK\sourceData\Polen\poland_streets2.csv";

            var json = JsonSerializer.Deserialize<PolishData>(File.ReadAllText(jsonPath));

            if (json?.name == null)
            {
                Console.WriteLine("❌ JSON bevat geen naamsectie");
                return;
            }

            ImportJsonFirstNames(json);
            ImportJsonLastNames(json);
            ImportStreets(streetsPath);

            Console.WriteLine("Polen ✓");
        }

        // -----------------------------------------------------
        // 1. VOORNAMEN
        // -----------------------------------------------------
        private void ImportJsonFirstNames(PolishData json)
        {
            // MAN
            if (json.name?.first_name_male != null)
            {
                foreach (var raw in json.name.first_name_male)
                {
                    try
                    {
                        string naam = Normalizer.Clean(raw);

                        if (_voornaamRepo.Exists(naam, "M", _landId))
                            continue;

                        _voornaamMgr.ValideerVoornaam(naam);
                        _voornaamRepo.Insert(naam, "M", 1, _landId);
                    }
                    catch { }
                }
            }

            // VROUW
            if (json.name?.first_name_female != null)
            {
                foreach (var raw in json.name.first_name_female)
                {
                    try
                    {
                        string naam = Normalizer.Clean(raw);

                        if (_voornaamRepo.Exists(naam, "F", _landId))
                            continue;

                        _voornaamMgr.ValideerVoornaam(naam);
                        _voornaamRepo.Insert(naam, "F", 1, _landId);
                    }
                    catch { }
                }
            }
        }

        // -----------------------------------------------------
        // 2. ACHTERNAMEN
        // -----------------------------------------------------
        private void ImportJsonLastNames(PolishData json)
        {
            if (json.name?.last_name == null)
                return;

            foreach (var raw in json.name.last_name)
            {
                try
                {
                    string naam = Normalizer.Clean(raw);

                    if (_achternaamRepo.Exists(naam, _landId))
                        continue;

                    _achternaamMgr.ValideerAchternaam(naam);
                    _achternaamRepo.Insert(naam, 1, _landId);
                }
                catch { }
            }
        }

        // -----------------------------------------------------
        // 3. STRATEN
        // -----------------------------------------------------
        private void ImportStreets(string path)
        {
            foreach (var row in CsvReader.Read(path))
            {
                try
                {
                    if (row.Length < 3) continue;

                    string gemeente = Normalizer.Clean(row[0]);
                    string straat = Normalizer.Clean(row[1]);
                    string wegtype = row[2].Trim().ToLower();

                    if (_gemeenteMgr.IsOngeldigeGemeente(gemeente)) continue;
                    if (_straatMgr.IsOngeldigeStraat(straat)) continue;
                    if (!_straatMgr.IsGeldigWegtype(wegtype)) continue;

                    int gemeenteId = _gemeenteRepo.InsertOfOphalen(gemeente, _landId);

                    // duplicate check
                    if (_straatRepo.Exists(gemeenteId, straat))
                        continue;

                    _straatRepo.Insert(gemeenteId, straat, wegtype);
                }
                catch { }
            }
        }

        // -----------------------------------------------------
        // JSON MODELS
        // -----------------------------------------------------
        public class PolishData
        {
            public PolishNames? name { get; set; }
        }

        public class PolishNames
        {
            public List<string>? first_name_male { get; set; }
            public List<string>? first_name_female { get; set; }
            public List<string>? last_name { get; set; }
        }
    }
}
