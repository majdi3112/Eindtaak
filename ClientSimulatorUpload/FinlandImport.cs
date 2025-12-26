using System;
using ClientSimulator_DL.Repository;
using ClientSimulatorUtils;
using ClientSimulator_BL.Manager;

namespace ClientSimulatorUpload
{
    public class FinlandImporter
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

        public FinlandImporter(int landId)
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
            Console.WriteLine("=== Finland importeren ===");

            ImportVoornamen(
                @"C:\Users\hp\Desktop\EINDTAAK\sourceData\Finland\etunimitilasto-2025-08-13-dvv_miehet_ens.txt",
                "M"
            );

            ImportVoornamen(
                @"C:\Users\hp\Desktop\EINDTAAK\sourceData\Finland\etunimitilasto-2025-08-13-dvv_naiset_ens.txt",
                "F"
            );

            ImportAchternamen(
                @"C:\Users\hp\Desktop\EINDTAAK\sourceData\Finland\sukunimitilasto-2025-08-13-dvv.txt"
            );

            ImportStraten(
                @"C:\Users\hp\Desktop\EINDTAAK\sourceData\Finland\finland_streets2.csv"
            );

            Console.WriteLine("Finland ✓");
        }

        // ------------------------------------------------------------
        // VOORNAMEN (TSV)
        // ------------------------------------------------------------
        private void ImportVoornamen(string path, string gender)
        {
            Console.WriteLine($"→ Voornamen ({gender}) laden uit: {path}");

            int toegevoegd = 0;
            int overgeslagen = 0;
            int fouten = 0;

            foreach (var parts in TxtReader.ReadTabSeparated(path))
            {
                try
                {
                    if (parts.Length < 2) { fouten++; continue; }

                    string naam = Normalizer.Clean(parts[0]);
                    string freqStr = parts[1].Replace(".", "").Replace(",", "");

                    if (!int.TryParse(freqStr, out int freq))
                    {
                        fouten++;
                        continue;
                    }

                    _voornaamMgr.ValideerVoornaam(naam);
                    _voornaamMgr.ValideerGeslacht(gender);

                    // Duplicate check
                    if (_voornaamRepo.Exists(naam, gender, _landId))
                    {
                        overgeslagen++;
                        continue;
                    }

                    freq = _voornaamMgr.NormaliseerFrequentie(freq);
                    _voornaamRepo.Insert(naam, gender, freq, _landId);
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

        // ------------------------------------------------------------
        // ACHTERNAMEN (TSV)
        // ------------------------------------------------------------
        private void ImportAchternamen(string path)
        {
            Console.WriteLine($"→ Achternamen laden uit: {path}");

            int toegevoegd = 0;
            int overgeslagen = 0;
            int fouten = 0;

            foreach (var parts in TxtReader.ReadTabSeparated(path))
            {
                try
                {
                    if (parts.Length < 2) { fouten++; continue; }

                    string naam = Normalizer.Clean(parts[0]);
                    string freqStr = parts[1].Replace(".", "").Replace(",", "");

                    if (!int.TryParse(freqStr, out int freq))
                    {
                        fouten++;
                        continue;
                    }

                    _achternaamMgr.ValideerAchternaam(naam);

                    if (_achternaamRepo.Exists(naam, _landId))
                    {
                        overgeslagen++;
                        continue;
                    }

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

        // ------------------------------------------------------------
        // STRATEN
        // ------------------------------------------------------------
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

                    if (!_straatMgr.IsGeldigWegtype(wegtype)) { overgeslagen++; continue; }

                    int gemeenteId = _gemeenteRepo.InsertOfOphalen(gemeente, _landId);

                    if (_straatRepo.Exists(gemeenteId, straat, wegtype)) { overgeslagen++; continue; }

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
