using Microsoft.Data.SqlClient;

namespace ClientSimulator_DL.Db
{
    public static class DbConnectionFactory
    {
        private static readonly string _connectionString =
            "Data Source=DESKTOP-S1KV2PJ\\SQLEXPRESS;Initial Catalog=ClientSimulatorDB;Integrated Security=True;Trust Server Certificate=True";

        public static SqlConnection Create()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
