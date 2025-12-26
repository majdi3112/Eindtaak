using ClientSimulator_BL.Interfaces;
using ClientSimulator_BL.Model;

namespace ClientSimulator_BL.Manager
{
    public class PersoonManager
    {
        private readonly VoornaamManager _voornaamMgr;
        private readonly AchternaamManager _achternaamMgr;
        private readonly GemeenteManager _gemeenteMgr;
        private readonly StraatManager _straatMgr;
        private readonly IPersoonRepository _persoonRepo;
        private readonly Random _random = new Random();

        public PersoonManager(
            VoornaamManager v,
            AchternaamManager a,
            GemeenteManager g,
            StraatManager s,
            IPersoonRepository p = null)
        {
            _voornaamMgr = v;
            _achternaamMgr = a;
            _gemeenteMgr = g;
            _straatMgr = s;
            _persoonRepo = p;
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
                Leeftijd = GenereerLeeftijd(),
                Huisnummer = GenereerHuisnummer()
            };
        }

        public Persoon Genereer(int landId, int minLeeftijd, int maxLeeftijd, string opdrachtgever)
        {
            var persoon = Genereer(landId);
            persoon.Leeftijd = GenereerLeeftijd(minLeeftijd, maxLeeftijd);
            persoon.Opdrachtgever = opdrachtgever;
            persoon.Huisnummer = GenereerHuisnummer();
            return persoon;
        }

        public void Opslaan(Persoon persoon)
        {
            if (_persoonRepo == null)
                throw new InvalidOperationException("PersoonRepository is niet geïnjecteerd");

            _persoonRepo.Insert(persoon);
        }

        private int GenereerLeeftijd(int minLeeftijd = 18, int maxLeeftijd = 90)
        {
            // Zorg voor geldige leeftijdsgrenzen
            minLeeftijd = Math.Max(0, minLeeftijd);
            maxLeeftijd = Math.Max(minLeeftijd + 1, maxLeeftijd);

            return _random.Next(minLeeftijd, maxLeeftijd + 1);
        }

        private string GenereerHuisnummer()
        {
            int nummer = _random.Next(1, 1000); // Huisnummers van 1 tot 999

            // 30% kans op busnummer (A, B, C, etc.)
            if (_random.Next(100) < 30)
            {
                char bus = (char)('A' + _random.Next(3)); // A, B, of C
                return $"{nummer}{bus}";
            }

            return nummer.ToString();
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
