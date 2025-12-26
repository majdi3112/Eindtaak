using System;
using System.Collections.Generic;
using ClientSimulator_BL.Interfaces;
using ClientSimulator_BL.Model;
using ClientSimulator_DL.Db;
using Microsoft.Data.SqlClient;

namespace ClientSimulator_DL.Repository
{
    public class SimulatieInstellingenRepository : ISimulatieInstellingenRepository
    {
        public int Insert(SimulatieInstellingen instellingen)
        {
            using var conn = DbConnectionFactory.Create();
            conn.Open();

            var sql = """
                INSERT INTO SimulatieInstellingen (
                    LandId, AantalKlanten, MinLeeftijd, MaxLeeftijd, Opdrachtgever,
                    MaxHuisnummer, PercentageLetters, PercentageBusnummer, SimulatieDatum
                ) OUTPUT INSERTED.SimulatieId
                VALUES (
                    @landId, @aantalKlanten, @minLeeftijd, @maxLeeftijd, @opdrachtgever,
                    @maxHuisnummer, @percentageLetters, @percentageBusnummer, @simulatieDatum
                )
            """;

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@landId", instellingen.LandId);
            cmd.Parameters.AddWithValue("@aantalKlanten", instellingen.AantalKlanten);
            cmd.Parameters.AddWithValue("@minLeeftijd", instellingen.MinLeeftijd);
            cmd.Parameters.AddWithValue("@maxLeeftijd", instellingen.MaxLeeftijd);
            cmd.Parameters.AddWithValue("@opdrachtgever", instellingen.Opdrachtgever);
            cmd.Parameters.AddWithValue("@maxHuisnummer", instellingen.MaxHuisnummer);
            cmd.Parameters.AddWithValue("@percentageLetters", instellingen.PercentageLetters);
            cmd.Parameters.AddWithValue("@percentageBusnummer", instellingen.PercentageBusnummer);
            cmd.Parameters.AddWithValue("@simulatieDatum", instellingen.SimulatieDatum);

            return (int)cmd.ExecuteScalar();
        }

        public SimulatieInstellingen GetById(int simulatieId)
        {
            using var conn = DbConnectionFactory.Create();
            conn.Open();

            var sql = """
                SELECT SimulatieId, LandId, AantalKlanten, MinLeeftijd, MaxLeeftijd,
                       Opdrachtgever, MaxHuisnummer, PercentageLetters, PercentageBusnummer, SimulatieDatum
                FROM SimulatieInstellingen
                WHERE SimulatieId = @simulatieId
            """;

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@simulatieId", simulatieId);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;

            return new SimulatieInstellingen
            {
                SimulatieId = (int)reader["SimulatieId"],
                LandId = (int)reader["LandId"],
                AantalKlanten = (int)reader["AantalKlanten"],
                MinLeeftijd = (int)reader["MinLeeftijd"],
                MaxLeeftijd = (int)reader["MaxLeeftijd"],
                Opdrachtgever = reader["Opdrachtgever"].ToString(),
                MaxHuisnummer = (int)reader["MaxHuisnummer"],
                PercentageLetters = (int)reader["PercentageLetters"],
                PercentageBusnummer = (int)reader["PercentageBusnummer"],
                SimulatieDatum = (DateTime)reader["SimulatieDatum"]
            };
        }

        public List<SimulatieInstellingen> GetByLand(int landId)
        {
            var result = new List<SimulatieInstellingen>();

            using var conn = DbConnectionFactory.Create();
            conn.Open();

            var sql = """
                SELECT SimulatieId, LandId, AantalKlanten, MinLeeftijd, MaxLeeftijd,
                       Opdrachtgever, MaxHuisnummer, PercentageLetters, PercentageBusnummer, SimulatieDatum
                FROM SimulatieInstellingen
                WHERE LandId = @landId
                ORDER BY SimulatieDatum DESC
            """;

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@landId", landId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new SimulatieInstellingen
                {
                    SimulatieId = (int)reader["SimulatieId"],
                    LandId = (int)reader["LandId"],
                    AantalKlanten = (int)reader["AantalKlanten"],
                    MinLeeftijd = (int)reader["MinLeeftijd"],
                    MaxLeeftijd = (int)reader["MaxLeeftijd"],
                    Opdrachtgever = reader["Opdrachtgever"].ToString(),
                    MaxHuisnummer = (int)reader["MaxHuisnummer"],
                    PercentageLetters = (int)reader["PercentageLetters"],
                    PercentageBusnummer = (int)reader["PercentageBusnummer"],
                    SimulatieDatum = (DateTime)reader["SimulatieDatum"]
                });
            }

            return result;
        }

        public List<SimulatieInstellingen> GetByOpdrachtgever(string opdrachtgever)
        {
            var result = new List<SimulatieInstellingen>();

            using var conn = DbConnectionFactory.Create();
            conn.Open();

            var sql = """
                SELECT SimulatieId, LandId, AantalKlanten, MinLeeftijd, MaxLeeftijd,
                       Opdrachtgever, MaxHuisnummer, PercentageLetters, PercentageBusnummer, SimulatieDatum
                FROM SimulatieInstellingen
                WHERE Opdrachtgever = @opdrachtgever
                ORDER BY SimulatieDatum DESC
            """;

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@opdrachtgever", opdrachtgever);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new SimulatieInstellingen
                {
                    SimulatieId = (int)reader["SimulatieId"],
                    LandId = (int)reader["LandId"],
                    AantalKlanten = (int)reader["AantalKlanten"],
                    MinLeeftijd = (int)reader["MinLeeftijd"],
                    MaxLeeftijd = (int)reader["MaxLeeftijd"],
                    Opdrachtgever = reader["Opdrachtgever"].ToString(),
                    MaxHuisnummer = (int)reader["MaxHuisnummer"],
                    PercentageLetters = (int)reader["PercentageLetters"],
                    PercentageBusnummer = (int)reader["PercentageBusnummer"],
                    SimulatieDatum = (DateTime)reader["SimulatieDatum"]
                });
            }

            return result;
        }

        public List<SimulatieInstellingen> GetAll()
        {
            var result = new List<SimulatieInstellingen>();

            using var conn = DbConnectionFactory.Create();
            conn.Open();

            var sql = """
                SELECT SimulatieId, LandId, AantalKlanten, MinLeeftijd, MaxLeeftijd,
                       Opdrachtgever, MaxHuisnummer, PercentageLetters, PercentageBusnummer, SimulatieDatum
                FROM SimulatieInstellingen
                ORDER BY SimulatieDatum DESC
            """;

            using var cmd = new SqlCommand(sql, conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new SimulatieInstellingen
                {
                    SimulatieId = (int)reader["SimulatieId"],
                    LandId = (int)reader["LandId"],
                    AantalKlanten = (int)reader["AantalKlanten"],
                    MinLeeftijd = (int)reader["MinLeeftijd"],
                    MaxLeeftijd = (int)reader["MaxLeeftijd"],
                    Opdrachtgever = reader["Opdrachtgever"].ToString(),
                    MaxHuisnummer = (int)reader["MaxHuisnummer"],
                    PercentageLetters = (int)reader["PercentageLetters"],
                    PercentageBusnummer = (int)reader["PercentageBusnummer"],
                    SimulatieDatum = (DateTime)reader["SimulatieDatum"]
                });
            }

            return result;
        }
    }
}

