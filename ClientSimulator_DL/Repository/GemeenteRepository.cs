using ClientSimulator_DL.Db;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientSimulator_BL.Model;
using ClientSimulator_BL.Interfaces;

namespace ClientSimulator_DL.Repository
{
    public class GemeenteRepository : IGemeenteRepository
    {
        public int InsertOfOphalen(string naam, int landId)
        {
            using var conn = DbConnectionFactory.Create();
            conn.Open();

            // 1) Bestaat het al?
            var selectCmd = new SqlCommand(
                "SELECT GemeenteId FROM Gemeente WHERE Naam = @naam AND LandId = @landId",
                conn);
            selectCmd.Parameters.AddWithValue("@naam", naam);
            selectCmd.Parameters.AddWithValue("@landId", landId);

            var result = selectCmd.ExecuteScalar();
            if (result != null)
                return (int)result;

            // 2) Anders: nieuw toevoegen
            var insertCmd = new SqlCommand(
                "INSERT INTO Gemeente (Naam, LandId) OUTPUT INSERTED.GemeenteId VALUES (@naam, @landId)",
                conn);
            insertCmd.Parameters.AddWithValue("@naam", naam);
            insertCmd.Parameters.AddWithValue("@landId", landId);

            return (int)insertCmd.ExecuteScalar();
        }

        public Gemeente GetRandom(int landId)
        {
            using var conn = DbConnectionFactory.Create();
            conn.Open();

            var sql = """
                SELECT TOP 1 GemeenteId, Naam, LandId
                FROM Gemeente
                WHERE LandId = @landId
                ORDER BY NEWID()
            """;

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@landId", landId);

            using var r = cmd.ExecuteReader();
            if (!r.Read())
                throw new Exception("Geen gemeente gevonden");

            return new Gemeente
            {
                Id = (int)r["GemeenteId"],
                Naam = r["Naam"].ToString(),
                LandId = (int)r["LandId"]
            };
        }

        public List<Gemeente> GetByLand(int landId)
        {
            var result = new List<Gemeente>();

            using var conn = DbConnectionFactory.Create();
            conn.Open();

            var sql = """
                SELECT GemeenteId, Naam, LandId
                FROM Gemeente
                WHERE LandId = @landId
                ORDER BY Naam
            """;

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@landId", landId);

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                result.Add(new Gemeente
                {
                    Id = (int)r["GemeenteId"],
                    Naam = r["Naam"].ToString(),
                    LandId = (int)r["LandId"]
                });
            }

            return result;
        }
    }
}
