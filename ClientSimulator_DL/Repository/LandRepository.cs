using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using ClientSimulator_DL.Db;
using ClientSimulator_BL.Model;


namespace ClientSimulator_DL.Repository
{
    public class LandRepository
    {
        public int InsertOfOphalen(string naam)   
        {
            using var conn = DbConnectionFactory.Create();
            conn.Open();

            // 1) Bestaat het?
            var selectCmd = new SqlCommand(
                "SELECT LandId FROM Land WHERE Naam = @naam", conn);

            selectCmd.Parameters.AddWithValue("@naam", naam);

            var result = selectCmd.ExecuteScalar();
            if (result != null)
                return (int)result;

            // 2) Anders toevoegen
            var insertCmd = new SqlCommand(
                "INSERT INTO Land (Naam) OUTPUT INSERTED.LandId VALUES (@naam)", conn);

            insertCmd.Parameters.AddWithValue("@naam", naam);

            return (int)insertCmd.ExecuteScalar();
        }

        public Land GetById(int landId)
        {
            using var conn = DbConnectionFactory.Create();
            conn.Open();

            var cmd = new SqlCommand("SELECT LandId, Naam FROM Land WHERE LandId = @landId", conn);
            cmd.Parameters.AddWithValue("@landId", landId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Land
                {
                    LandId = (int)reader["LandId"],
                    Naam = reader["Naam"].ToString()
                };
            }

            return null;
        }

        public List<Land> GetAll()
        {
            var landen = new List<Land>();

            using var conn = DbConnectionFactory.Create();
            conn.Open();

            var cmd = new SqlCommand("SELECT LandId, Naam FROM Land ORDER BY Naam", conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                landen.Add(new Land
                {
                    LandId = (int)reader["LandId"],
                    Naam = reader["Naam"].ToString()
                });
            }

            return landen;
        }
    }
}