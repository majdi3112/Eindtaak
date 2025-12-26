using System.Collections.Generic;

namespace ClientSimulator_BL.Model
{
    public class SimulatieStatistieken
    {
        public int TotaalKlanten { get; set; }
        public double GemiddeldeLeeftijd { get; set; }
        public int MinimumLeeftijd { get; set; }
        public int MaximumLeeftijd { get; set; }
        public Persoon JongsteKlant { get; set; }
        public Persoon OudsteKlant { get; set; }
        public List<NaamFrequentie> TopVoornamen { get; set; } = new();
        public List<NaamFrequentie> TopAchternamen { get; set; } = new();
        public List<GemeenteVerdeling> GemeenteVerdeling { get; set; } = new();
    }

    public class NaamFrequentie
    {
        public string Naam { get; set; }
        public int Aantal { get; set; }
    }

    public class GemeenteVerdeling
    {
        public string GemeenteNaam { get; set; }
        public int AantalKlanten { get; set; }
        public double Percentage { get; set; }
    }
}
