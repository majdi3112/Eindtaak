using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClientSimulator_BL.Model;

namespace ClientSimulator_BL.Interfaces
{
    public interface IStraatRepository
    {
        Straat GetRandom(int landId);
        Straat GetRandomByGemeente(int gemeenteId);
        bool Exists(int gemeenteId, string naam);
        void Insert(int gemeenteId, string naam, string wegtype);
    }
}
