using ClientSimulator_BL.Interfaces;
using ClientSimulator_BL.Model;
using ClientSimulator_DL.Db;
using Microsoft.Data.SqlClient;

namespace ClientSimulator_DL.Repository
{
    public class StraatRepository : IStraatRepository
    {
        public Straat GetRandom(int landId)
        {
            using var conn = DbConnectionFactory.Create();
            conn.Open();

            var sql = """
                SELECT TOP 1 s.Naam, s.HighwayType
                FROM Straat s
                JOIN Gemeente g ON s.GemeenteId = g.GemeenteId
                WHERE g.LandId = @landId
                ORDER BY NEWID()
            """;

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@landId", landId);

            using var r = cmd.ExecuteReader();
            if (!r.Read())
                throw new Exception("Geen straat gevonden");

            return new Straat
            {
                Naam = r["Naam"].ToString(),
                Wegtype = r["HighwayType"].ToString()
            };
        }

        public Straat GetRandomByGemeente(int gemeenteId)
        {
            using var conn = DbConnectionFactory.Create();
            conn.Open();

            var sql = """
                SELECT TOP 1 s.Naam, s.HighwayType
                FROM Straat s
                WHERE s.GemeenteId = @gemeenteId
                ORDER BY NEWID()
            """;

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@gemeenteId", gemeenteId);

            using var r = cmd.ExecuteReader();
            if (!r.Read())
                throw new Exception("Geen straat gevonden voor deze gemeente");

            return new Straat
            {
                Naam = r["Naam"].ToString(),
                Wegtype = r["HighwayType"].ToString()
            };
        }

        public bool Exists(int gemeenteId, string naam, string wegtype)
        {
            using var conn = DbConnectionFactory.Create();
            conn.Open();

            var sql = """
                SELECT COUNT(*) FROM Straat
                WHERE GemeenteId = @gemeenteId AND Naam = @naam AND HighwayType = @wegtype
            """;

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@gemeenteId", gemeenteId);
            cmd.Parameters.AddWithValue("@naam", naam);
            cmd.Parameters.AddWithValue("@wegtype", wegtype);

            return (int)cmd.ExecuteScalar() > 0;
        }

        public void Insert(int gemeenteId, string naam, string wegtype)
        {
            using var conn = DbConnectionFactory.Create();
            conn.Open();

            var sql = """
                INSERT INTO Straat (GemeenteId, Naam, HighwayType)
                VALUES (@gemeenteId, @naam, @wegtype)
            """;

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@gemeenteId", gemeenteId);
            cmd.Parameters.AddWithValue("@naam", naam);
            cmd.Parameters.AddWithValue("@wegtype", wegtype);

            cmd.ExecuteNonQuery();
        }
    }
}
