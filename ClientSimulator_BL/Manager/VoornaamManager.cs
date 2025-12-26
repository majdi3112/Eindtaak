using ClientSimulator_BL.Interfaces;
using ClientSimulator_BL.Model;

namespace ClientSimulator_BL.Manager
{
    public class VoornaamManager
    {
        private readonly IVoornaamRepository _repo;

        public VoornaamManager(IVoornaamRepository repo)
        {
            _repo = repo;
        }

        public Voornaam GeefRandom(int landId)
        {
            return _repo.GetRandom(landId);
        }

        public void ValideerVoornaam(string naam)
        {
            if (string.IsNullOrWhiteSpace(naam))
                throw new ArgumentException("Voornaam mag niet leeg zijn");

            if (naam.Length < 2)
                throw new ArgumentException("Voornaam moet minstens 2 karakters bevatten");

            if (naam.Length > 50)
                throw new ArgumentException("Voornaam mag maximum 50 karakters bevatten");

            // Controleer op ongeldige karakters
            if (naam.Contains("@") || naam.Contains("#") || naam.Contains("$") ||
                naam.Contains("%") || naam.Contains("&") || naam.Contains("*"))
                throw new ArgumentException("Voornaam bevat ongeldige karakters");

            // Controleer op alleen cijfers
            if (int.TryParse(naam, out _))
                throw new ArgumentException("Voornaam mag niet alleen uit cijfers bestaan");
        }

        public void ValideerGeslacht(string geslacht)
        {
            if (string.IsNullOrWhiteSpace(geslacht))
                throw new ArgumentException("Geslacht mag niet leeg zijn");

            if (geslacht != "M" && geslacht != "F")
                throw new ArgumentException("Geslacht moet 'M' of 'F' zijn");
        }

        public int NormaliseerFrequentie(int frequentie)
        {
            // Zorg ervoor dat frequentie minimaal 1 is
            return Math.Max(1, frequentie);
        }
    }
}
