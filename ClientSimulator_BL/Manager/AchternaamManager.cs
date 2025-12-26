using ClientSimulator_BL.Interfaces;
using ClientSimulator_BL.Model;

namespace ClientSimulator_BL.Manager
{
    public class AchternaamManager
    {
        private readonly IAchternaamRepository _repo;

        public AchternaamManager(IAchternaamRepository repo)
        {
            _repo = repo;
        }

        public Achternaam GeefRandom(int landId)
        {
            return _repo.GetRandom(landId);
        }

        public void ValideerAchternaam(string naam)
        {
            if (string.IsNullOrWhiteSpace(naam))
                throw new ArgumentException("Achternaam mag niet leeg zijn");

            if (naam.Length < 2)
                throw new ArgumentException("Achternaam moet minstens 2 karakters bevatten");


            // Controleer op ongeldige karakters
            if (naam.Contains("@") || naam.Contains("#") || naam.Contains("$") ||
                naam.Contains("%") || naam.Contains("&") || naam.Contains("*"))
                throw new ArgumentException("Achternaam bevat ongeldige karakters");

            // Controleer op alleen cijfers
            if (int.TryParse(naam, out _))
                throw new ArgumentException("Achternaam mag niet alleen uit cijfers bestaan");
        }

        public int NormaliseerFrequentie(int frequentie)
        {
            // Zorg ervoor dat frequentie minimaal 1 is
            return Math.Max(1, frequentie);
        }
    }
}
