using ClientSimulator_BL.Interfaces;
using ClientSimulator_BL.Model;
using ClientSimulator_DL.Db;
using Microsoft.Data.SqlClient;

namespace ClientSimulator_DL.Repository
{
    public class PersoonRepository : IPersoonRepository
    {
        public void Insert(Persoon persoon)
        {
            using var conn = DbConnectionFactory.Create();
            conn.Open();

            var sql = """
                INSERT INTO Persoon (
                    Voornaam, Achternaam, Geslacht, Straat, Gemeente, Land,
                    Leeftijd, Huisnummer, Opdrachtgever, GeboorteDatum, HuidigeLeeftijd
                ) VALUES (
                    @voornaam, @achternaam, @geslacht, @straat, @gemeente, @land,
                    @leeftijd, @huisnummer, @opdrachtgever, @geboorteDatum, @huidigeLeeftijd
                )
            """;

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@voornaam", persoon.Voornaam);
            cmd.Parameters.AddWithValue("@achternaam", persoon.Achternaam);
            cmd.Parameters.AddWithValue("@geslacht", persoon.Geslacht);
            cmd.Parameters.AddWithValue("@straat", persoon.Straat);
            cmd.Parameters.AddWithValue("@gemeente", persoon.Gemeente);
            cmd.Parameters.AddWithValue("@land", persoon.Land);
            cmd.Parameters.AddWithValue("@leeftijd", persoon.Leeftijd);
            cmd.Parameters.AddWithValue("@huisnummer", persoon.Huisnummer);
            cmd.Parameters.AddWithValue("@opdrachtgever", persoon.Opdrachtgever);
            cmd.Parameters.AddWithValue("@geboorteDatum", persoon.GeboorteDatum);
            cmd.Parameters.AddWithValue("@huidigeLeeftijd", persoon.HuidigeLeeftijd);

            cmd.ExecuteNonQuery();
        }

        // Legacy methods - these should not be used anymore
        public Voornaam GetRandomVoornaam(int landId)
        {
            throw new NotImplementedException("Use VoornaamRepository instead");
        }

        public Achternaam GetRandomAchternaam(int landId)
        {
            throw new NotImplementedException("Use AchternaamRepository instead");
        }

        public Gemeente GetRandomGemeente(int landId)
        {
            throw new NotImplementedException("Use GemeenteRepository instead");
        }

        public Straat GetRandomStraat(int landId)
        {
            throw new NotImplementedException("Use StraatRepository instead");
        }
    }
}
