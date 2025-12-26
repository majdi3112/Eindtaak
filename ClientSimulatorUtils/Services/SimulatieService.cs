using ClientSimulator_BL.Manager;
using ClientSimulator_BL.Model;
using ClientSimulator_DL.Repository;
using ClientSimulator_DL.Db;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClientSimulatorUtils.Services
{
    public class SimulatieService
    {
        public SimulatieService()
        {
            // Services coördineren managers voor complexe operaties
        }

        public List<Persoon> VoerSimulatieUit(int landId, int aantalKlanten, int minLeeftijd, int maxLeeftijd, string opdrachtgever)
        {
            return VoerSimulatieUit(landId, aantalKlanten, minLeeftijd, maxLeeftijd, opdrachtgever, null, 999, 10, 30);
        }

        public List<Persoon> VoerSimulatieUit(
            int landId, 
            int aantalKlanten, 
            int minLeeftijd, 
            int maxLeeftijd, 
            string opdrachtgever,
            Dictionary<int, double> gemeentePercentages,
            int maxHuisnummer,
            int percentageLetters,
            int percentageBusnummer)
        {
            var personen = new List<Persoon>();

            // Sla simulatie instellingen eerst op
            var instellingenRepo = new SimulatieInstellingenRepository();
            var instellingen = new SimulatieInstellingen
            {
                LandId = landId,
                AantalKlanten = aantalKlanten,
                MinLeeftijd = minLeeftijd,
                MaxLeeftijd = maxLeeftijd,
                Opdrachtgever = opdrachtgever,
                MaxHuisnummer = maxHuisnummer,
                PercentageLetters = percentageLetters,
                PercentageBusnummer = percentageBusnummer,
                SimulatieDatum = DateTime.Now
            };

            int simulatieId = instellingenRepo.Insert(instellingen);

            // Gebruik managers voor persoon generatie
            var voornaamRepo = new VoornaamRepository();
            var achternaamRepo = new AchternaamRepository();
            var gemeenteRepo = new GemeenteRepository();
            var straatRepo = new StraatRepository();
            var landRepo = new LandRepository();

            var voornaamMgr = new VoornaamManager(voornaamRepo);
            var achternaamMgr = new AchternaamManager(achternaamRepo);
            var gemeenteMgr = new GemeenteManager(gemeenteRepo);
            var straatMgr = new StraatManager(straatRepo);

            // Haal land naam op
            var land = landRepo.GetById(landId);
            var landNaam = land?.Naam ?? "Unknown";

            var persoonRepo = new PersoonRepository();
            var persoonMgr = new PersoonManager(voornaamMgr, achternaamMgr, gemeenteMgr, straatMgr, persoonRepo);

            // Bereid gemeente selectie voor als percentages zijn opgegeven
            List<(int GemeenteId, double Percentage)> gemeenteSelectie = null;
            if (gemeentePercentages != null && gemeentePercentages.Count > 0)
            {
                gemeenteSelectie = gemeentePercentages
                    .Where(kvp => kvp.Value > 0)
                    .Select(kvp => (kvp.Key, kvp.Value))
                    .ToList();
                
                // Normaliseer percentages (zorg dat ze optellen tot 100%)
                double totaalPercentage = gemeenteSelectie.Sum(g => g.Percentage);
                if (totaalPercentage > 0)
                {
                    gemeenteSelectie = gemeenteSelectie
                        .Select(g => (g.GemeenteId, g.Percentage / totaalPercentage * 100))
                        .ToList();
                }
            }

            Random random = new Random();

            for (int i = 0; i < aantalKlanten; i++)
            {
                // Selecteer gemeente op basis van percentages indien opgegeven
                Gemeente geselecteerdeGemeente;
                if (gemeenteSelectie != null && gemeenteSelectie.Count > 0)
                {
                    // Gewogen random selectie op basis van percentages
                    double randomWaarde = random.NextDouble() * 100;
                    double cumulatiefPercentage = 0;
                    int geselecteerdeGemeenteId = gemeenteSelectie[0].GemeenteId;

                    foreach (var gemeente in gemeenteSelectie)
                    {
                        cumulatiefPercentage += gemeente.Percentage;
                        if (randomWaarde <= cumulatiefPercentage)
                        {
                            geselecteerdeGemeenteId = gemeente.GemeenteId;
                            break;
                        }
                    }

                    // Haal gemeente op
                    var gemeenten = gemeenteMgr.GetByLand(landId);
                    geselecteerdeGemeente = gemeenten.FirstOrDefault(g => g.Id == geselecteerdeGemeenteId);
                    if (geselecteerdeGemeente == null)
                        geselecteerdeGemeente = gemeenteMgr.GeefRandom(landId);
                }
                else
                {
                    geselecteerdeGemeente = gemeenteMgr.GeefRandom(landId);
                }

                // Genereer persoon
                var persoon = persoonMgr.Genereer(landId, minLeeftijd, maxLeeftijd, opdrachtgever);
                persoon.Land = landNaam;
                persoon.SimulatieId = simulatieId; // Koppel aan simulatie
                
                // Overschrijf gemeente en straat als specifieke gemeente is geselecteerd
                if (geselecteerdeGemeente != null)
                {
                    persoon.Gemeente = geselecteerdeGemeente.Naam;
                    // Kies straat uit de geselecteerde gemeente
                    var straat = straatMgr.GeefRandomByGemeente(geselecteerdeGemeente.Id);
                    persoon.Straat = straat.Naam;
                }

                // Genereer huisnummer met instellingen
                persoon.Huisnummer = GenereerHuisnummer(random, maxHuisnummer, percentageLetters, percentageBusnummer);

                personen.Add(persoon);

                // Sla persoon op in database
                persoonMgr.Opslaan(persoon);
            }

            return personen;
        }

        public List<SimulatieInstellingen> GetSimulatiesByLand(int landId)
        {
            var repo = new SimulatieInstellingenRepository();
            return repo.GetByLand(landId);
        }

        public List<SimulatieInstellingen> GetSimulatiesByOpdrachtgever(string opdrachtgever)
        {
            var repo = new SimulatieInstellingenRepository();
            return repo.GetByOpdrachtgever(opdrachtgever);
        }

        public List<Persoon> GetPersonenBySimulatieId(int simulatieId)
        {
            var repo = new PersoonRepository();
            return repo.GetBySimulatieId(simulatieId);
        }

        private string GenereerHuisnummer(Random random, int maxHuisnummer, int percentageLetters, int percentageBusnummer)
        {
            int nummer = random.Next(1, maxHuisnummer + 1);

            // Bepaal of letter toevoegen
            if (random.Next(100) < percentageLetters)
            {
                char letter = (char)('A' + random.Next(26));
                return $"{nummer}{letter}";
            }

            // Bepaal of busnummer toevoegen
            if (random.Next(100) < percentageBusnummer)
            {
                char bus = (char)('A' + random.Next(3)); // A, B, of C
                return $"{nummer}{bus}";
            }

            return nummer.ToString();
        }

        public List<Gemeente> GetGemeentenByLand(int landId)
        {
            var gemeenteRepo = new GemeenteRepository();
            var gemeenteMgr = new GemeenteManager(gemeenteRepo);
            return gemeenteMgr.GetByLand(landId);
        }

        public SimulatieStatistieken BerekenStatistieken(List<Persoon> personen, SimulatieInstellingen instellingen = null)
        {
            if (personen.Count == 0)
                return new SimulatieStatistieken();

            var stats = new SimulatieStatistieken
            {
                TotaalKlanten = personen.Count,
                GemiddeldeLeeftijd = personen.Average(p => p.Leeftijd),
                GemiddeldeLeeftijdHuidigeDatum = personen.Average(p => p.HuidigeLeeftijd),
                MinimumLeeftijd = personen.Min(p => p.Leeftijd),
                MaximumLeeftijd = personen.Max(p => p.Leeftijd),
                JongsteKlant = personen.OrderBy(p => p.Leeftijd).First(),
                OudsteKlant = personen.OrderByDescending(p => p.Leeftijd).First(),
                Instellingen = instellingen
            };

            // Top 10 voornamen
            stats.TopVoornamen = personen
                .GroupBy(p => p.Voornaam)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => new NaamFrequentie { Naam = g.Key, Aantal = g.Count() })
                .ToList();

            // Top 10 achternamen
            stats.TopAchternamen = personen
                .GroupBy(p => p.Achternaam)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => new NaamFrequentie { Naam = g.Key, Aantal = g.Count() })
                .ToList();

            // Gemeente verdeling met aantal straten
            var gemeenteRepo = new GemeenteRepository();
            var straatRepo = new StraatRepository();
            var gemeenteMgr = new GemeenteManager(gemeenteRepo);

            stats.GemeenteVerdeling = personen
                .GroupBy(p => p.Gemeente)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g =>
                {
                    // Zoek gemeente ID om aantal straten op te halen
                    int aantalStraten = 0;
                    try
                    {
                        // Probeer gemeente te vinden in database
                        var gemeenten = gemeenteMgr.GetByLand(instellingen?.LandId ?? 0);
                        var gemeente = gemeenten.FirstOrDefault(ge => ge.Naam == g.Key);
                        if (gemeente != null)
                        {
                            // Tel aantal straten voor deze gemeente
                            // We moeten een query maken om aantal straten te tellen
                            aantalStraten = TelStratenPerGemeente(gemeente.Id);
                        }
                    }
                    catch { }

                    return new GemeenteVerdeling
                    {
                        GemeenteNaam = g.Key,
                        AantalKlanten = g.Count(),
                        Percentage = (double)g.Count() / personen.Count * 100,
                        AantalStraten = aantalStraten
                    };
                })
                .ToList();

            return stats;
        }

        private int TelStratenPerGemeente(int gemeenteId)
        {
            using var conn = ClientSimulator_DL.Db.DbConnectionFactory.Create();
            conn.Open();

            var sql = "SELECT COUNT(*) FROM Straat WHERE GemeenteId = @gemeenteId";
            using var cmd = new Microsoft.Data.SqlClient.SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@gemeenteId", gemeenteId);

            return (int)cmd.ExecuteScalar();
        }
    }

}
