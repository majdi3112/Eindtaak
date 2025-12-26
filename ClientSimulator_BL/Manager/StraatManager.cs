using ClientSimulator_BL.Interfaces;
using ClientSimulator_BL.Model;

namespace ClientSimulator_BL.Manager
{
    public class StraatManager
    {
        private readonly IStraatRepository _repo;

        public StraatManager(IStraatRepository repo)
        {
            _repo = repo;
        }

        public Straat GeefRandom(int landId)
        {
            return _repo.GetRandom(landId);
        }

        public Straat GeefRandomByGemeente(int gemeenteId)
        {
            return _repo.GetRandomByGemeente(gemeenteId);
        }

        public bool IsOngeldigeStraat(string straat)
        {
            if (string.IsNullOrWhiteSpace(straat))
                return true;

            if (straat.Length < 2)
                return true;

            // Filter ongeldige straten
            string lower = straat.ToLower();
            if (lower.Contains("unknown") || lower.Contains("(unknown)") ||
                lower.Contains("test") || lower.Contains("dummy") ||
                lower.Contains("null") || lower.Contains("n/a") ||
                lower.Contains("unnamed") || lower.Contains("no name"))
                return true;

            // Controleer op alleen cijfers
            if (int.TryParse(straat.Trim(), out _))
                return true;

            return false;
        }

        public bool IsGeldigWegtype(string wegtype)
        {
            if (string.IsNullOrWhiteSpace(wegtype))
                return false;

            // Geldige OSM highway types volgens de opdracht
            string[] geldigeTypes = {
                "motorway", "trunk", "primary", "secondary", "tertiary",
                "unclassified", "residential", "living_street", "service",
                "track", "footway", "bridleway", "steps", "path",
                "cycleway", "pedestrian", "raceway"
            };

            return geldigeTypes.Contains(wegtype.ToLower());
        }

        public string NormaliseerWegtype(string wegtype)
        {
            if (string.IsNullOrWhiteSpace(wegtype))
                return "residential";

            return wegtype.ToLower().Trim();
        }
    }
}
