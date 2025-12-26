using ClientSimulator_BL.Manager;
using ClientSimulator_BL.Model;
using ClientSimulator_DL.Repository;
using System.Collections.Generic;

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
            var personen = new List<Persoon>();

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

            for (int i = 0; i < aantalKlanten; i++)
            {
                var persoon = persoonMgr.Genereer(landId, minLeeftijd, maxLeeftijd, opdrachtgever);
                persoon.Land = landNaam; // Stel correct land in
                personen.Add(persoon);

                // Sla persoon op in database
                persoonMgr.Opslaan(persoon);
            }

            return personen;
        }

        public List<Gemeente> GetGemeentenByLand(int landId)
        {
            var gemeenteRepo = new GemeenteRepository();
            var gemeenteMgr = new GemeenteManager(gemeenteRepo);
            return gemeenteMgr.GetByLand(landId);
        }

        public SimulatieStatistieken BerekenStatistieken(List<Persoon> personen)
        {
            if (personen.Count == 0)
                return new SimulatieStatistieken();

            var stats = new SimulatieStatistieken
            {
                TotaalKlanten = personen.Count,
                GemiddeldeLeeftijd = personen.Average(p => p.Leeftijd),
                MinimumLeeftijd = personen.Min(p => p.Leeftijd),
                MaximumLeeftijd = personen.Max(p => p.Leeftijd),
                JongsteKlant = personen.OrderBy(p => p.Leeftijd).First(),
                OudsteKlant = personen.OrderByDescending(p => p.Leeftijd).First()
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

            // Gemeente verdeling
            stats.GemeenteVerdeling = personen
                .GroupBy(p => p.Gemeente)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => new GemeenteVerdeling
                {
                    GemeenteNaam = g.Key,
                    AantalKlanten = g.Count(),
                    Percentage = (double)g.Count() / personen.Count * 100
                })
                .ToList();

            return stats;
        }
    }

}
