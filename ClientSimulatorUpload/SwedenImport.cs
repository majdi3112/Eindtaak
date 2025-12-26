using System;
using System.IO;
using ClientSimulator_DL.Repository;
using ClientSimulator_BL.Manager;
using ClientSimulatorUtils;

namespace ClientSimulatorUpload
{
    public class SwedenImporter
    {
        private readonly VoornaamRepository _voornaamRepo = new();
        private readonly AchternaamRepository _achternaamRepo = new();
        private readonly GemeenteRepository _gemeenteRepo = new();
        private readonly StraatRepository _straatRepo = new();

        private readonly VoornaamManager _voornaamMgr;
        private readonly AchternaamManager _achternaamMgr;
        private readonly GemeenteManager _gemeenteMgr;
        private readonly StraatManager _straatMgr;

        private readonly int _landId;

        public SwedenImporter(int landId)
        {
            _landId = landId;

            _voornaamMgr = new VoornaamManager(new VoornaamRepository());
            _achternaamMgr = new AchternaamManager(new AchternaamRepository());
            _gemeenteMgr = new GemeenteManager(new GemeenteRepository());
            _straatMgr = new StraatManager(new StraatRepository());
        }

        public void Import()
        {
            Console.WriteLine("=== Zweden importeren ===");

            ImportFirstNames(
                @"C:\Users\hp\Desktop\EINDTAAK\sourceData\Zweden\namn-med-minst-tva-barare-31-december-2022_Tilltalsnamn_män.txt",
                "M"
            );

            ImportFirstNames(
                @"C:\Users\hp\Desktop\EINDTAAK\sourceData\Zweden\namn-med-minst-tva-barare-31-december-2022_Tilltalsnamn_kvinnor.txt",
                "F"
            );

            ImportLastNames(
                @"C:\Users\hp\Desktop\EINDTAAK\sourceData\Zweden\namn-med-minst-tva-barare-31-december-2022_Efternamn.txt"
            );

            ImportStreets(
                @"C:\Users\hp\Desktop\EINDTAAK\sourceData\Zweden\sweden_streets2.csv"
            );

            Console.WriteLine("Zweden ✓");
        }

        // -------------------------------
        // VOORNAMEN (TSV)
        // -------------------------------
        private void ImportFirstNames(string path, string gender)
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
                    if (string.IsNullOrWhiteSpace(naam)) continue;

                    // Parse frequency (remove dots)
                    string freqStr = parts[1].Replace(".", "").Replace(",", "");
                    if (!int.TryParse(freqStr, out int freq))
                    {
                        fouten++;
                        continue;
                    }

                    _voornaamMgr.ValideerVoornaam(naam);
                    _voornaamMgr.ValideerGeslacht(gender);

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
                    Console.WriteLine($"[ZW-VOORNAAM FOUT] {ex.Message}");
                }
            }

            Console.WriteLine($"   ✓ Toegevoegd: {toegevoegd}, Overgeslagen: {overgeslagen}, Fouten: {fouten}");
        }

        // -------------------------------
        // ACHTERNAMEN (TSV)
        // -------------------------------
        private void ImportLastNames(string path)
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

                    // Parse frequency (remove dots)
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
                    Console.WriteLine($"[ZW-ACHTERNAAM FOUT] {ex.Message}");
                }
            }

            Console.WriteLine($"   ✓ Toegevoegd: {toegevoegd}, Overgeslagen: {overgeslagen}, Fouten: {fouten}");
        }

        // -------------------------------
        // STRATEN
        // -------------------------------
        private void ImportStreets(string path)
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
                    string type = row[2].Trim().ToLower();

                    if (_gemeenteMgr.IsOngeldigeGemeente(gemeente)) { overgeslagen++; continue; }
                    if (_straatMgr.IsOngeldigeStraat(straat)) { overgeslagen++; continue; }
                    if (!_straatMgr.IsGeldigWegtype(type)) { overgeslagen++; continue; }

                    int gemeenteId = _gemeenteRepo.InsertOfOphalen(gemeente, _landId);

                    if (_straatRepo.Exists(gemeenteId, straat))
                    {
                        overgeslagen++;
                        continue;
                    }

                    _straatRepo.Insert(gemeenteId, straat, type);
                    toegevoegd++;
                }
                catch (Exception ex)
                {
                    fouten++;
                    Console.WriteLine($"[ZW-STRAAT FOUT] {ex.Message}");
                }
            }

            Console.WriteLine($"   ✓ Toegevoegd: {toegevoegd}, Overgeslagen: {overgeslagen}, Fouten: {fouten}");
        }
    }
}
