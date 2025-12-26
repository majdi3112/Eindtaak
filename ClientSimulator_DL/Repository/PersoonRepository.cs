using ClientSimulator_BL.Interfaces;
using ClientSimulator_BL.Model;
using System.IO;

namespace ClientSimulator_DL
{
    public class PersoonRepository : IPersoonRepository
    {
        public Voornaam GetRandomVoornaam(int landId)
        {
            throw new NotImplementedException("PersoonRepository.GetRandomVoornaam is not implemented");
        }

        public Achternaam GetRandomAchternaam(int landId)
        {
            throw new NotImplementedException("PersoonRepository.GetRandomAchternaam is not implemented");
        }

        public Gemeente GetRandomGemeente(int landId)
        {
            throw new NotImplementedException("PersoonRepository.GetRandomGemeente is not implemented");
        }

        public Straat GetRandomStraat(int landId)
        {
            throw new NotImplementedException("PersoonRepository.GetRandomStraat is not implemented");
        }
    }
}
