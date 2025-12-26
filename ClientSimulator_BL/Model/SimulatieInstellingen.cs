using System;

namespace ClientSimulator_BL.Model
{
    public class SimulatieInstellingen
    {
        public int SimulatieId { get; set; }
        public int LandId { get; set; }
        public int AantalKlanten { get; set; }
        public int MinLeeftijd { get; set; }
        public int MaxLeeftijd { get; set; }
        public string Opdrachtgever { get; set; }
        public int MaxHuisnummer { get; set; }
        public int PercentageLetters { get; set; }
        public int PercentageBusnummer { get; set; }
        public DateTime SimulatieDatum { get; set; }
    }
}

