using System;
using System.Collections.Generic;
using System.IO;
using ClientSimulator_DL.Repository;
using ClientSimulatorUtils;
using ClientSimulator_BL.Manager;

namespace ClientSimulatorUpload
{
    public class SpainImporter
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

        public SpainImporter(int landId)
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
            Console.WriteLine("=== Spanje importeren ===");

            ImportFirstNames(@"C:\Users\hp\Desktop\EINDTAAK\sourceData\Spanje\nombres_por_edad_media_hombres.txt", "M");
            ImportFirstNames(@"C:\Users\hp\Desktop\EINDTAAK\sourceData\Spanje\nombres_por_edad_media_mujeres.txt", "F");

            ImportLastNames(@"C:\Users\hp\Desktop\EINDTAAK\sourceData\Spanje\apellidos_frecuencia_100mas.txt");
            ImportLastNames(@"C:\Users\hp\Desktop\EINDTAAK\sourceData\Spanje\apellidos_frecuencia_20mas.txt");

            ImportStreets(@"C:\Users\hp\Desktop\EINDTAAK\sourceData\Spanje\spain_streets2.csv");

            Console.WriteLine("Spanje ✓");
        }

        // -----------------------------------------------------------
        // 1. PARSE VOORNAMEN (TSV format)
        // -----------------------------------------------------------
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
                    if (parts.Length < 3) { fouten++; continue; }

                    string naam = Normalizer.Clean(parts[1]);
                    string freqStr = parts[2].Replace(".", "").Replace(",", "");

                    if (!int.TryParse(freqStr, out int freq))
                    {
                        fouten++;
                        continue;
                    }

                    naam = Normalizer.Clean(naam);

                    if (_voornaamRepo.Exists(naam, gender, _landId))
                    {
                        overgeslagen++;
                        continue;
                    }

                    _voornaamMgr.ValideerVoornaam(naam);
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

        // -----------------------------------------------------------
        // 2. PARSE ACHTERNAMEN (TSV format)
        // -----------------------------------------------------------
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
                    if (parts.Length < 3) { fouten++; continue; }

                    string achternaam = Normalizer.Clean(parts[1]);
                    string freqStr = parts[2].Replace(".", "").Replace(",", "");

                    if (!int.TryParse(freqStr, out int freq))
                    {
                        fouten++;
                        continue;
                    }

                    achternaam = Normalizer.Clean(achternaam);

                    if (_achternaamRepo.Exists(achternaam, _landId))
                    {
                        overgeslagen++;
                        continue;
                    }

                    _achternaamMgr.ValideerAchternaam(achternaam);
                    freq = _achternaamMgr.NormaliseerFrequentie(freq);
                    _achternaamRepo.Insert(achternaam, freq, _landId);
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

        // -----------------------------------------------------------
        // 3. STRATEN CSV
        // -----------------------------------------------------------
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
                    string wegtype = row[2].Trim().ToLower();

                    if (_gemeenteMgr.IsOngeldigeGemeente(gemeente)) { overgeslagen++; continue; }
                    if (_straatMgr.IsOngeldigeStraat(straat)) { overgeslagen++; continue; }
                    if (!_straatMgr.IsGeldigWegtype(wegtype)) { overgeslagen++; continue; }

                    int gemeenteId = _gemeenteRepo.InsertOfOphalen(gemeente, _landId);

                    if (_straatRepo.Exists(gemeenteId, straat, wegtype))
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
