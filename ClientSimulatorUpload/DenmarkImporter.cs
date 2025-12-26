using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ClientSimulator_DL.Repository;
using ClientSimulatorUtils;
using ClientSimulator_BL.Manager;

namespace ClientSimulatorUpload
{
    public class DenmarkImporter
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

        public DenmarkImporter(int landId)
        {
            _landId = landId;

            // Eerst repositories maken!
            _voornaamRepo = new VoornaamRepository();
            _achternaamRepo = new AchternaamRepository();
            _gemeenteRepo = new GemeenteRepository();
            _straatRepo = new StraatRepository();

            // Dan managers
            _voornaamMgr = new VoornaamManager(_voornaamRepo);
            _achternaamMgr = new AchternaamManager(_achternaamRepo);
            _gemeenteMgr = new GemeenteManager(_gemeenteRepo);
            _straatMgr = new StraatManager(_straatRepo);
        }

        public void Import()
        {
            Console.WriteLine("=== Denemarken data importeren ===");

            ImportVoornaamTxt(
                @"C:\Users\hp\Desktop\EINDTAAK\sourceData\Denemarken\fornavne 2025 - mænd (3+) - med overskrifter.txt",
                "M"
            );

            ImportVoornaamTxt(
                @"C:\Users\hp\Desktop\EINDTAAK\sourceData\Denemarken\fornavne 2025 - kvinder (3+) - med overskrifter.txt",
                "F"
            );

            ImportAchternaamTxt(
                @"C:\Users\hp\Desktop\EINDTAAK\sourceData\Denemarken\efternavne 2025 (3+) - med overskrifter.txt"
            );

            ImportStreetsCsv(
                @"C:\Users\hp\Desktop\EINDTAAK\sourceData\Denemarken\denmark_streets2.csv"
            );

            Console.WriteLine("Beëindigd: Denemarken ✓");
        }

        // ------------------------------------------------------
        // 1. TSV VOORNAMEN (tab-separated)
        // ------------------------------------------------------
        private void ImportVoornaamTxt(string path, string geslacht)
        {
            Console.WriteLine($"→ Voornamen ({geslacht}) laden uit: {path}");

            int toegevoegd = 0;
            int overgeslagen = 0;
            int fouten = 0;

            foreach (var parts in TxtReader.ReadTabSeparated(path))
            {
                try
                {
                    if (parts.Length < 2) { fouten++; continue; }

                    string naam = Normalizer.Clean(parts[0]);
                    if (string.IsNullOrWhiteSpace(naam)) continue;

                    // Parse frequency (remove dots for thousands)
                    string freqStr = parts[1].Replace(".", "").Replace(",", "");
                    if (!int.TryParse(freqStr, out int freq))
                    {
                        fouten++;
                        continue;
                    }

                    _voornaamMgr.ValideerVoornaam(naam);

                    // DUPLICATE CHECK
                    if (_voornaamRepo.Exists(naam, geslacht, _landId))
                    {
                        overgeslagen++;
                        continue;
                    }

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

        // ------------------------------------------------------
        // 2. TSV ACHTERNAMEN (tab-separated)
        // ------------------------------------------------------
        private void ImportAchternaamTxt(string path)
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
                    if (string.IsNullOrWhiteSpace(naam)) continue;

                    // Parse frequency (remove dots for thousands)
                    string freqStr = parts[1].Replace(".", "").Replace(",", "");
                    if (!int.TryParse(freqStr, out int freq))
                    {
                        fouten++;
                        continue;
                    }

                    _achternaamMgr.ValideerAchternaam(naam);

                    // DUPLICATE CHECK
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

        // ------------------------------------------------------
        // 3. STRATEN CSV
        // ------------------------------------------------------
        private void ImportStreetsCsv(string path)
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

                    // Skip "(unknown)" gemeenten volgens opdracht
                    if (gemeente.Equals("(unknown)", StringComparison.OrdinalIgnoreCase) ||
                        gemeente.Equals("unknown", StringComparison.OrdinalIgnoreCase))
                    {
                        overgeslagen++;
                        continue;
                    }

                    // Verwijder "Kommune" van gemeente naam volgens opdracht
                    if (gemeente.EndsWith(" Kommune", StringComparison.OrdinalIgnoreCase))
                    {
                        gemeente = gemeente.Substring(0, gemeente.Length - " Kommune".Length).Trim();
                    }

                    if (_gemeenteMgr.IsOngeldigeGemeente(gemeente)) { overgeslagen++; continue; }
                    if (_straatMgr.IsOngeldigeStraat(straat)) { overgeslagen++; continue; }
                    if (!_straatMgr.IsGeldigWegtype(wegtype)) { overgeslagen++; continue; }

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
