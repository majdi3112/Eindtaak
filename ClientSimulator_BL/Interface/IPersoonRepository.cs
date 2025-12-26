using System.Collections.Generic;
using ClientSimulator_BL.Model;

namespace ClientSimulator_BL.Interfaces
{
    public interface IPersoonRepository
    {
        void Insert(Persoon persoon);
        List<Persoon> GetBySimulatieId(int simulatieId);

        // Legacy methods - these should not be used anymore
        Voornaam GetRandomVoornaam(int landId);
        Achternaam GetRandomAchternaam(int landId);
        Gemeente GetRandomGemeente(int landId);
        Straat GetRandomStraat(int landId);
    }
}
