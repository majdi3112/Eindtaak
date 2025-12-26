using System;
using System.Collections.Generic;
using System.Linq;
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
            // Gebruik gewogen random selectie op basis van frequentie
            return GetRandomWeighted(landId);
        }

        // Gewogen random selectie op basis van frequentie
        public Achternaam GetRandomWeighted(int landId)
        {
            using var conn = DbConnectionFactory.Create();
            conn.Open();

            // Haal alle achternamen met frequenties op
            string sql = """
                SELECT Naam, Frequentie
                FROM Achternaam
                WHERE LandId = @landId
            """;

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@landId", landId);

            var namen = new List<(string Naam, int Frequentie)>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                namen.Add((
                    reader["Naam"].ToString(),
                    (int)reader["Frequentie"]
                ));
            }

            if (namen.Count == 0)
                throw new Exception("Geen achternamen gevonden");

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
                    return new Achternaam
                    {
                        Naam = naam.Naam
                    };
                }
            }

            // Fallback naar laatste naam
            return new Achternaam
            {
                Naam = namen.Last().Naam
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
