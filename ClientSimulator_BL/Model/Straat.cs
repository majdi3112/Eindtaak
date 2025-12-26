using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSimulator_BL.Model
{

    public class Straat
    {
        public int Id { get; set; }
        public string Naam { get; set; }
        public string Wegtype { get; set; }
        public int GemeenteId { get; set; }
    }
}
