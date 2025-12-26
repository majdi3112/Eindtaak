using ClientSimulator_BL.Interfaces;
using ClientSimulator_BL.Model;
using ClientSimulator_DL.Db;
using Microsoft.Data.SqlClient;

namespace ClientSimulator_DL.Repository
{
    public class AchternaamRepository : IAchternaamRepository
    {
        public Achternaam GetRandom(int landId)
        {
            using var conn = DbConnectionFactory.Create();
            conn.Open();

            string sql = """
                SELECT TOP 1 Naam
                FROM Achternaam
                WHERE LandId = @landId
                ORDER BY NEWID()
            """;

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@landId", landId);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                throw new Exception("Geen achternaam gevonden");

            return new Achternaam
            {
                Naam = reader["Naam"].ToString()
            };
        }

        public bool Exists(string naam, int landId)
        {
            using var conn = DbConnectionFactory.Create();
            conn.Open();

            var sql = "SELECT COUNT(*) FROM Achternaam WHERE Naam = @naam AND LandId = @landId";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@naam", naam);
            cmd.Parameters.AddWithValue("@landId", landId);

            return (int)cmd.ExecuteScalar() > 0;
        }

        public void Insert(string naam, int frequentie, int landId)
        {
            using var conn = DbConnectionFactory.Create();
            conn.Open();

            var sql = "INSERT INTO Achternaam (Naam, Frequentie, LandId) VALUES (@naam, @frequentie, @landId)";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@naam", naam);
            cmd.Parameters.AddWithValue("@frequentie", frequentie);
            cmd.Parameters.AddWithValue("@landId", landId);

            cmd.ExecuteNonQuery();
        }
    }
}
