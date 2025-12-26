using ClientSimulator_BL.Model;

namespace ClientSimulator_BL.Manager
{
    public class PersoonManager
    {
        private readonly VoornaamManager _voornaamMgr;
        private readonly AchternaamManager _achternaamMgr;
        private readonly GemeenteManager _gemeenteMgr;
        private readonly StraatManager _straatMgr;
        private readonly Random _random = new Random();

        public PersoonManager(
            VoornaamManager v,
            AchternaamManager a,
            GemeenteManager g,
            StraatManager s)
        {
            _voornaamMgr = v;
            _achternaamMgr = a;
            _gemeenteMgr = g;
            _straatMgr = s;
        }

        public Persoon Genereer(int landId)
        {
            var v = _voornaamMgr.GeefRandom(landId);
            var a = _achternaamMgr.GeefRandom(landId);
            var g = _gemeenteMgr.GeefRandom(landId);
            var s = _straatMgr.GeefRandom(landId);

            return new Persoon
            {
                Voornaam = v.Naam,
                Achternaam = a.Naam,
                Geslacht = v.Geslacht,
                Gemeente = g.Naam,
                Straat = s.Naam,
                Land = "Unknown", // Dit moet later worden opgehaald uit de database
                Leeftijd = GenereerLeeftijd()
            };
        }

        public Persoon Genereer(int landId, int minLeeftijd, int maxLeeftijd, string opdrachtgever)
        {
            var persoon = Genereer(landId);
            persoon.Leeftijd = GenereerLeeftijd(minLeeftijd, maxLeeftijd);
            persoon.Opdrachtgever = opdrachtgever;
            return persoon;
        }

        private int GenereerLeeftijd(int minLeeftijd = 18, int maxLeeftijd = 90)
        {
            // Zorg voor geldige leeftijdsgrenzen
            minLeeftijd = Math.Max(0, minLeeftijd);
            maxLeeftijd = Math.Max(minLeeftijd + 1, maxLeeftijd);

            return _random.Next(minLeeftijd, maxLeeftijd + 1);
        }

        public void ValideerPersoon(Persoon persoon)
        {
            if (persoon == null)
                throw new ArgumentNullException(nameof(persoon));

            if (string.IsNullOrWhiteSpace(persoon.Voornaam))
                throw new ArgumentException("Voornaam is verplicht");

            if (string.IsNullOrWhiteSpace(persoon.Achternaam))
                throw new ArgumentException("Achternaam is verplicht");

            if (string.IsNullOrWhiteSpace(persoon.Geslacht))
                throw new ArgumentException("Geslacht is verplicht");

            if (persoon.Leeftijd < 0 || persoon.Leeftijd > 150)
                throw new ArgumentException("Ongeldige leeftijd");
        }
    }
}
