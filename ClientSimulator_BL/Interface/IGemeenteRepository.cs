using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientSimulator_BL.Model;

namespace ClientSimulator_BL.Interfaces
{
    public interface IGemeenteRepository
    {
        Gemeente GetRandom(int landId);
        List<Gemeente> GetByLand(int landId);
    }
}
