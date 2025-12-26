using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSimulator_BL.Model
{
    public class Gemeente
    {
        public int Id { get; set; }
        public string Naam { get; set; }
        public string Land { get; set; }
        public int LandId { get; set; }
    }

}
