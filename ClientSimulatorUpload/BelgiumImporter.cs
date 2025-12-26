using System;
using ClientSimulator_DL.Repository;
using ClientSimulator_BL.Manager;
using ClientSimulatorUtils;

namespace ClientSimulatorUpload
{
    public class BelgiumImporter
    {
        private readonly VoornaamManager _voornaamMgr;
        private readonly AchternaamManager _achternaamMgr;
        private readonly GemeenteManager _gemeenteMgr;
        private readonly StraatManager _straatMgr;

        private readonly VoornaamRepository _voornaamRepo;
        private readonly AchternaamRepository _achternaamRepo;
        private readonly GemeenteRepository _gemeenteRepo;
        private readonly StraatRepository _straatRepo;

        private readonly int _landId;

        public BelgiumImporter(int landId)
        {
            _landId = landId;

            _voornaamMgr = new VoornaamManager(_voornaamRepo);
            _achternaamMgr = new AchternaamManager(_achternaamRepo);
            _gemeenteMgr = new GemeenteManager(_gemeenteRepo);
            _straatMgr = new StraatManager(_straatRepo);

            _voornaamRepo = new VoornaamRepository();
            _achternaamRepo = new AchternaamRepository();
            _gemeenteRepo = new GemeenteRepository();
            _straatRepo = new StraatRepository();
        }

        public void Import()
        {
            Console.WriteLine("=== België data importeren ===\n");

            ImportVoornamen(
                @"C:\Users\hp\Desktop\EINDTAAK\sourceData\België\mannennamen_belgie.csv",
                "M"
            );

            ImportVoornamen(
                @"C:\Users\hp\Desktop\EINDTAAK\sourceData\België\vrouwennamen_belgie.csv",
                "F"
            );

            ImportAchternamen(
                @"C:\Users\hp\Desktop\EINDTAAK\sourceData\België\Familienamen_2024_Belgie.csv"
            );

            ImportStraten(
                @"C:\Users\hp\Desktop\EINDTAAK\sourceData\België\belgium_streets2.csv"
            );

            Console.WriteLine("\nBeëindigd: België ✓");
        }


        // ============================================================
        // 1. VOORNAMEN
        // ============================================================
        private void ImportVoornamen(string path, string geslacht)
        {
            Console.WriteLine($"→ Voornamen ({geslacht}) laden uit: {path}");

            int toegevoegd = 0;
            int overgeslagen = 0;
            int fouten = 0;

            foreach (var row in CsvReader.Read(path))
            {
                try
                {
                    if (row.Length < 2) { fouten++; continue; }

                    string naam = Normalizer.Clean(row[1]);
                    if (string.IsNullOrWhiteSpace(naam)) continue;

                    _voornaamMgr.ValideerVoornaam(naam);

                    // DUPLICATE CHECK
                    if (_voornaamRepo.Exists(naam, geslacht, _landId))
                    {
                        overgeslagen++;
                        continue;
                    }

                    int freq = (row.Length >= 3 && int.TryParse(row[2], out int f))
                        ? f : 1;

                    freq = _voornaamMgr.NormaliseerFrequentie(freq);

                    _voornaamRepo.Insert(naam, geslacht, freq, _landId);
                    toegevoegd++;
                }
                catch (Exception ex)
                {
                    fouten++;
                    Console.WriteLine($"[VOORNAAM FOUT] {ex.Message}");
                }
            }

            Console.WriteLine($"   ✓ Toegevoegd: {toegevoegd}, Overgeslagen: {overgeslagen}, Fouten: {fouten}");
        }


        // ============================================================
        // 2. ACHTERNAMEN
        // ============================================================
        private void ImportAchternamen(string path)
        {
            Console.WriteLine($"→ Achternamen laden uit: {path}");

            int toegevoegd = 0;
            int overgeslagen = 0;
            int fouten = 0;

            foreach (var row in CsvReader.Read(path))
            {
                try
                {
                    if (row.Length < 2) { fouten++; continue; }

                    string naam = Normalizer.Clean(row[1]);
                    if (string.IsNullOrWhiteSpace(naam)) continue;

                    _achternaamMgr.ValideerAchternaam(naam);

                    // DUPLICATE CHECK
                    if (_achternaamRepo.Exists(naam, _landId))
                    {
                        overgeslagen++;
                        continue;
                    }

                    int freq = (row.Length >= 3 && int.TryParse(row[2], out int f))
                        ? f : 1;

                    freq = _achternaamMgr.NormaliseerFrequentie(freq);

                    _achternaamRepo.Insert(naam, freq, _landId);
                    toegevoegd++;
                }
                catch (Exception ex)
                {
                    fouten++;
                    Console.WriteLine($"[ACHTERNAAM FOUT] {ex.Message}");
                }
            }

            Console.WriteLine($"   ✓ Toegevoegd: {toegevoegd}, Overgeslagen: {overgeslagen}, Fouten: {fouten}");
        }


        // ============================================================
        // 3. STRATEN + GEMEENTEN
        // ============================================================
        private void ImportStraten(string path)
        {
            Console.WriteLine($"→ Straten laden uit: {path}");

            int toegevoegd = 0;
            int overgeslagen = 0;
            int fouten = 0;

            foreach (var row in CsvReader.Read(path))
            {
                try
                {
                    if (row.Length < 3) { fouten++; continue; }

                    string gemeente = Normalizer.Clean(row[0]);
                    string straat = Normalizer.Clean(row[1]);
                    string wegtype = row[2].Trim().ToLower();

                    if (_gemeenteMgr.IsOngeldigeGemeente(gemeente)) continue;
                    if (_straatMgr.IsOngeldigeStraat(straat)) continue;
                    if (!_straatMgr.IsGeldigWegtype(wegtype)) continue;

                    int gemeenteId = _gemeenteRepo.InsertOfOphalen(gemeente, _landId);

                    // DUPLICATE CHECK
                    if (_straatRepo.Exists(gemeenteId, straat))
                    {
                        overgeslagen++;
                        continue;
                    }

                    _straatRepo.Insert(gemeenteId, straat, wegtype);
                    toegevoegd++;
                }
                catch (Exception ex)
                {
                    fouten++;
                    Console.WriteLine($"[STRAAT FOUT] {ex.Message}");
                }
            }

            Console.WriteLine($"   ✓ Toegevoegd: {toegevoegd}, Overgeslagen: {overgeslagen}, Fouten: {fouten}");
        }
    }
}
