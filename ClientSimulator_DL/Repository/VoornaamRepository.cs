using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using ClientSimulator_DL.Db;
using ClientSimulator_BL.Model;
using ClientSimulator_BL.Interfaces;



namespace ClientSimulator_DL.Repository
{
   public class VoornaamRepository : IVoornaamRepository
    {

        public void Insert(string naam, string gender, int freq, int landId)
        {
            using var conn = DbConnectionFactory.Create();
            conn.Open();

            var cmd = new SqlCommand(
                @"INSERT INTO Voornaam (Naam, Geslacht, Frequentie, LandId)
                  VALUES (@n, @g, @f, @l)", conn);

            cmd.Parameters.AddWithValue("@n", naam);
            cmd.Parameters.AddWithValue("@g", gender);
            cmd.Parameters.AddWithValue("@f", freq);
            cmd.Parameters.AddWithValue("@l", landId);

            cmd.ExecuteNonQuery();
        }
        public bool Exists(string naam, string gender, int landId)
        {
            using var conn = DbConnectionFactory.Create();
            conn.Open();

            var cmd = new SqlCommand(
                @"SELECT COUNT(*) 
          FROM Voornaam 
          WHERE Naam = @n AND Geslacht = @g AND LandId = @l", conn);

            cmd.Parameters.AddWithValue("@n", naam);
            cmd.Parameters.AddWithValue("@g", gender);
            cmd.Parameters.AddWithValue("@l", landId);

            return (int)cmd.ExecuteScalar() > 0;
        }
        public Voornaam GetRandom(int landId)
        {
            using var conn = DbConnectionFactory.Create();
            conn.Open();

            string sql = """
        SELECT TOP 1 *
        FROM Voornaam
        WHERE LandId = @landId
        ORDER BY NEWID()
    """;

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@landId", landId);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                throw new Exception("Geen voornaam gevonden");

            return new Voornaam
            {
                Naam = reader["Naam"].ToString(),
                Geslacht = reader["Geslacht"].ToString()
            };
        }


    }
}
