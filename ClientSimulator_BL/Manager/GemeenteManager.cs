using ClientSimulator_BL.Interfaces;
using ClientSimulator_BL.Model;
using System.Text;

namespace ClientSimulator_BL.Manager
{
    public class GemeenteManager
    {
        private readonly IGemeenteRepository _repo;

        public GemeenteManager(IGemeenteRepository repo)
        {
            _repo = repo;
        }

        public Gemeente GeefRandom(int landId)
        {
            return _repo.GetRandom(landId);
        }

        public List<Gemeente> GetByLand(int landId)
        {
            return _repo.GetByLand(landId);
        }

        public bool IsOngeldigeGemeente(string gemeente)
        {
            if (string.IsNullOrWhiteSpace(gemeente))
                return true;

            if (gemeente.Length < 2)
                return true;

            // Filter ongeldige gemeenten
            string lower = gemeente.ToLower();
            if (lower.Contains("unknown") || lower.Contains("(unknown)") ||
                lower.Contains("test") || lower.Contains("dummy") ||
                lower.Contains("null") || lower.Contains("n/a"))
                return true;

            // Controleer op alleen cijfers
            if (int.TryParse(gemeente.Trim(), out _))
                return true;

            return false;
        }

        public string MaakSchoon(string gemeente)
        {
            if (string.IsNullOrWhiteSpace(gemeente))
                return gemeente;

            // Verwijder speciale tekens maar behoud letters, spaties en apostrofs
            var sb = new StringBuilder();
            foreach (char c in gemeente)
            {
                if (char.IsLetter(c) || char.IsWhiteSpace(c) || c == '\'' || c == '-')
                    sb.Append(c);
            }

            return sb.ToString().Trim();
        }
    }
}
