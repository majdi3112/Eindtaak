using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSimulator_BL.Model
{
    public class Persoon
    {
        public string Voornaam { get; set; }
        public string Achternaam { get; set; }
        public string Geslacht { get; set; }
        public int Leeftijd { get; set; }

        public string Straat { get; set; }
        public string Huisnummer { get; set; }
        public string Gemeente { get; set; }
        public string Land { get; set; }

        public string Opdrachtgever { get; set; }
        public DateTime AanmaakDatum { get; set; } = DateTime.Now;

        // Berekende properties
        public DateTime GeboorteDatum => AanmaakDatum.AddYears(-Leeftijd);
        public int HuidigeLeeftijd => DateTime.Now.Year - GeboorteDatum.Year;
    }
}
