using System;
using System.IO;
using ClientSimulator_DL.Repository;
using ClientSimulatorUtils;
using ClientSimulator_BL.Manager;

namespace ClientSimulatorUpload
{
    public class SwitzerlandImporter
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

        public SwitzerlandImporter(int landId)
        {
            _landId = landId;

            // Gebruik dezelfde repository instances die als fields zijn gedeclareerd
            _voornaamMgr = new VoornaamManager(_voornaamRepo);
            _achternaamMgr = new AchternaamManager(_achternaamRepo);
            _gemeenteMgr = new GemeenteManager(_gemeenteRepo);
            _straatMgr = new StraatManager(_straatRepo);
        }

        public void Import()
        {
            Console.WriteLine("=== Zwitserland importeren ===");

            ImportVoornamen(@"C:\Users\hp\Desktop\EINDTAAK\sourceData\Zwitserland\su-q-01.04.00.12_firstname.txt");
            ImportAchternamen(@"C:\Users\hp\Desktop\EINDTAAK\sourceData\Zwitserland\su-q-01.04.00.13_surnameCH.txt");
            ImportStraten(@"C:\Users\hp\Desktop\EINDTAAK\sourceData\Zwitserland\switzerland_streets2.csv");

            Console.WriteLine("Zwitserland ✓");
        }

        // --------------------------------------------------
        // 1. VOORNAMEN (TSV - female + male)
        // --------------------------------------------------
        private void ImportVoornamen(string path)
        {
            Console.WriteLine($"→ Voornamen laden uit: {path}");

            int toegevoegd = 0;
            int overgeslagen = 0;
            int fouten = 0;

            foreach (var parts in TxtReader.ReadTabSeparated(path))
            {
                try
                {
                    if (parts.Length < 3) { fouten++; continue; }

                    string naam = Normalizer.Clean(parts[0]);
                    if (string.IsNullOrWhiteSpace(naam)) continue;

                    int freqF = ParseSwissNumber(parts[1]);
                    int freqM = ParseSwissNumber(parts[2]);

                    _voornaamMgr.ValideerVoornaam(naam);

                    if (freqF > 0 && !_voornaamRepo.Exists(naam, "F", _landId))
                    {
                        freqF = _voornaamMgr.NormaliseerFrequentie(freqF);
                        _voornaamRepo.Insert(naam, "F", freqF, _landId);
                        toegevoegd++;
                    }
                    else if (freqF > 0)
                    {
                        overgeslagen++;
                    }

                    if (freqM > 0 && !_voornaamRepo.Exists(naam, "M", _landId))
                    {
                        freqM = _voornaamMgr.NormaliseerFrequentie(freqM);
                        _voornaamRepo.Insert(naam, "M", freqM, _landId);
                        toegevoegd++;
                    }
                    else if (freqM > 0)
                    {
                        overgeslagen++;
                    }
                }
                catch (Exception ex)
                {
                    fouten++;
                    Console.WriteLine($"[ZW-VOORNAAM FOUT] {ex.Message}");
                }
            }

            Console.WriteLine($"   ✓ Toegevoegd: {toegevoegd}, Overgeslagen: {overgeslagen}, Fouten: {fouten}");
        }

        // --------------------------------------------------
        // 2. ACHTERNAMEN (TSV)
        // --------------------------------------------------
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
                    if (parts.Length < 3) { fouten++; continue; }

                    string achternaam = Normalizer.Clean(parts[0]);
                    if (string.IsNullOrWhiteSpace(achternaam)) continue;

                    int freq = ParseSwissNumber(parts[2]);

                    _achternaamMgr.ValideerAchternaam(achternaam);

                    if (freq > 0 && !_achternaamRepo.Exists(achternaam, _landId))
                    {
                        freq = _achternaamMgr.NormaliseerFrequentie(freq);
                        _achternaamRepo.Insert(achternaam, freq, _landId);
                        toegevoegd++;
                    }
                    else if (freq > 0)
                    {
                        overgeslagen++;
                    }
                }
                catch (Exception ex)
                {
                    fouten++;
                    Console.WriteLine($"[ZW-ACHTERNAAM FOUT] {ex.Message}");
                }
            }

            Console.WriteLine($"   ✓ Toegevoegd: {toegevoegd}, Overgeslagen: {overgeslagen}, Fouten: {fouten}");
        }

        // --------------------------------------------------
        // 3. STRATEN (CSV)
        // --------------------------------------------------
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

                    if (_gemeenteMgr.IsOngeldigeGemeente(gemeente)) { overgeslagen++; continue; }
                    if (_straatMgr.IsOngeldigeStraat(straat)) { overgeslagen++; continue; }
                    if (!_straatMgr.IsGeldigWegtype(wegtype)) { overgeslagen++; continue; }

                    int gemeenteId = _gemeenteRepo.InsertOfOphalen(gemeente, _landId);

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
                    Console.WriteLine($"[ZW-STRAAT FOUT] {ex.Message}");
                }
            }

            Console.WriteLine($"   ✓ Toegevoegd: {toegevoegd}, Overgeslagen: {overgeslagen}, Fouten: {fouten}");
        }

        // --------------------------------------------------
        // Helpers
        // --------------------------------------------------
        private int ParseSwissNumber(string s)
        {
            if (string.IsNullOrWhiteSpace(s) || s == "*" || s == "-")
                return 0;

            return int.TryParse(s.Replace(".", ""), out int n) ? n : 0;
        }
    }
}
