using System.Collections.Generic;
using ClientSimulator_BL.Model;

namespace ClientSimulator_BL.Interfaces
{
    public interface ISimulatieInstellingenRepository
    {
        int Insert(SimulatieInstellingen instellingen);
        SimulatieInstellingen GetById(int simulatieId);
        List<SimulatieInstellingen> GetByLand(int landId);
        List<SimulatieInstellingen> GetByOpdrachtgever(string opdrachtgever);
        List<SimulatieInstellingen> GetAll();
    }
}

