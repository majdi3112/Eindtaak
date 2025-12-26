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
            // Gebruik gewogen random selectie op basis van frequentie
            return GetRandomWeighted(landId);
        }

        // Gewogen random selectie op basis van frequentie
        public Voornaam GetRandomWeighted(int landId)
        {
            using var conn = DbConnectionFactory.Create();
            conn.Open();

            // Haal alle namen met frequenties op
            string sql = """
                SELECT Naam, Geslacht, Frequentie
                FROM Voornaam
                WHERE LandId = @landId
            """;

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@landId", landId);

            var namen = new List<(string Naam, string Geslacht, int Frequentie)>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                namen.Add((
                    reader["Naam"].ToString(),
                    reader["Geslacht"].ToString(),
                    (int)reader["Frequentie"]
                ));
            }

            if (namen.Count == 0)
                throw new Exception("Geen voornamen gevonden");

            // Bereken totale frequentie
            int totaleFrequentie = namen.Sum(n => n.Frequentie);

            // Gewogen random selectie
            Random random = new Random();
            int randomWaarde = random.Next(1, totaleFrequentie + 1);
            int cumulatieveFrequentie = 0;

            foreach (var naam in namen)
            {
                cumulatieveFrequentie += naam.Frequentie;
                if (randomWaarde <= cumulatieveFrequentie)
                {
                    return new Voornaam
                    {
                        Naam = naam.Naam,
                        Geslacht = naam.Geslacht
                    };
                }
            }

            // Fallback naar laatste naam
            var laatste = namen.Last();
            return new Voornaam
            {
                Naam = laatste.Naam,
                Geslacht = laatste.Geslacht
            };
        }


    }
}
