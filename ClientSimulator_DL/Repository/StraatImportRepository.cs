using ClientSimulator_DL.Db;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSimulator_DL.Repository
{
    public class StraatImportRepository
    {
        public void Insert(int gemeenteId, string straat, string type)
        {
            using var conn = DbConnectionFactory.Create();
            conn.Open();

            var cmd = new SqlCommand(
                @"INSERT INTO Straat (GemeenteId, Naam, HighwayType)
                  VALUES (@g, @n, @t)", conn);

            cmd.Parameters.AddWithValue("@g", gemeenteId);
            cmd.Parameters.AddWithValue("@n", straat);
            cmd.Parameters.AddWithValue("@t", type);

            cmd.ExecuteNonQuery();
        }

        public bool Exists(int gemeenteId, string straat, string type)
        {
            using var conn = DbConnectionFactory.Create();
            conn.Open();

            var cmd = new SqlCommand(
                @"SELECT COUNT(*) 
                  FROM Straat 
                  WHERE GemeenteId = @g AND Naam = @n AND HighwayType = @t", conn);

            cmd.Parameters.AddWithValue("@g", gemeenteId);
            cmd.Parameters.AddWithValue("@n", straat);
            cmd.Parameters.AddWithValue("@t", type);

            return (int)cmd.ExecuteScalar() > 0;
        }
    }
}
