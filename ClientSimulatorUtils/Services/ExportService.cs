using ClientSimulator_BL.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ClientSimulatorUtils.Services
{
    public class ExportService
    {
        public void ExporteerNaarJson(List<Persoon> personen, SimulatieStatistieken stats, string bestandspad)
        {
            var exportData = new
            {
                metadata = new
                {
                    exportDate = System.DateTime.Now,
                    totalCustomers = personen.Count,
                    averageAge = stats.GemiddeldeLeeftijd,
                    minAge = stats.MinimumLeeftijd,
                    maxAge = stats.MaximumLeeftijd
                },
                statistics = new
                {
                    totalCustomers = stats.TotaalKlanten,
                    averageAge = stats.GemiddeldeLeeftijd,
                    youngestCustomer = new
                    {
                        name = $"{stats.JongsteKlant?.Voornaam} {stats.JongsteKlant?.Achternaam}",
                        age = stats.JongsteKlant?.Leeftijd
                    },
                    oldestCustomer = new
                    {
                        name = $"{stats.OudsteKlant?.Voornaam} {stats.OudsteKlant?.Achternaam}",
                        age = stats.OudsteKlant?.Leeftijd
                    },
                    topFirstNames = stats.TopVoornamen,
                    topLastNames = stats.TopAchternamen,
                    municipalityDistribution = stats.GemeenteVerdeling.Select(g => new
                    {
                        gemeenteNaam = g.GemeenteNaam,
                        aantalKlanten = g.AantalKlanten,
                        percentage = g.Percentage,
                        aantalStraten = g.AantalStraten
                    })
                },
                customers = personen.Select(p => new
                {
                    firstName = p.Voornaam,
                    lastName = p.Achternaam,
                    gender = p.Geslacht,
                    age = p.Leeftijd,
                    street = p.Straat,
                    houseNumber = p.Huisnummer,
                    city = p.Gemeente,
                    country = p.Land,
                    customer = p.Opdrachtgever,
                    birthDate = p.GeboorteDatum,
                    currentAge = p.HuidigeLeeftijd
                })
            };

            string json = System.Text.Json.JsonSerializer.Serialize(exportData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(bestandspad, json);
        }

        public void ExporteerNaarCsv(List<Persoon> personen, SimulatieStatistieken stats, string bestandspad, string scheidingsteken)
        {
            var sb = new StringBuilder();

            // Header
            sb.AppendLine($"Voornaam{scheidingsteken}Achternaam{scheidingsteken}Geslacht{scheidingsteken}Leeftijd{scheidingsteken}Straat{scheidingsteken}Huisnummer{scheidingsteken}Gemeente{scheidingsteken}Land{scheidingsteken}Opdrachtgever{scheidingsteken}Geboortedatum{scheidingsteken}HuidigeLeeftijd");

            // Data
            foreach (var persoon in personen)
            {
                sb.AppendLine($"{persoon.Voornaam}{scheidingsteken}{persoon.Achternaam}{scheidingsteken}{persoon.Geslacht}{scheidingsteken}{persoon.Leeftijd}{scheidingsteken}{persoon.Straat}{scheidingsteken}{persoon.Huisnummer}{scheidingsteken}{persoon.Gemeente}{scheidingsteken}{persoon.Land}{scheidingsteken}{persoon.Opdrachtgever}{scheidingsteken}{persoon.GeboorteDatum:dd-MM-yyyy}{scheidingsteken}{persoon.HuidigeLeeftijd}");
            }

            File.WriteAllText(bestandspad, sb.ToString());
        }

        public void ExporteerStatistieken(SimulatieStatistieken stats, string bestandspad)
        {
            var sb = new StringBuilder();

            sb.AppendLine("=== SIMULATIE STATISTIEKEN ===");
            sb.AppendLine($"Export datum: {System.DateTime.Now:dd-MM-yyyy HH:mm:ss}");
            sb.AppendLine();
            sb.AppendLine("=== KLANTEN OVERZICHT ===");
            sb.AppendLine($"Totaal aantal klanten: {stats.TotaalKlanten}");
            sb.AppendLine();
            sb.AppendLine("=== LEEFTIJD ANALYSE ===");
            sb.AppendLine($"Gemiddelde leeftijd bij simulatie: {stats.GemiddeldeLeeftijd:F1} jaar");
            sb.AppendLine($"Gemiddelde leeftijd op huidige datum: {stats.GemiddeldeLeeftijdHuidigeDatum:F1} jaar");
            sb.AppendLine($"Minimum leeftijd: {stats.MinimumLeeftijd} jaar");
            sb.AppendLine($"Maximum leeftijd: {stats.MaximumLeeftijd} jaar");
            sb.AppendLine($"Jongste klant: {stats.JongsteKlant?.Voornaam} {stats.JongsteKlant?.Achternaam} ({stats.JongsteKlant?.Leeftijd} jaar)");
            sb.AppendLine($"Oudste klant: {stats.OudsteKlant?.Voornaam} {stats.OudsteKlant?.Achternaam} ({stats.OudsteKlant?.Leeftijd} jaar)");
            sb.AppendLine();
            sb.AppendLine("=== VOORNAMEN (Top 10) ===");
            foreach (var item in stats.TopVoornamen)
            {
                sb.AppendLine($"{item.Naam}: {item.Aantal} keer");
            }
            sb.AppendLine();
            sb.AppendLine("=== ACHTERNAMEN (Top 10) ===");
            foreach (var item in stats.TopAchternamen)
            {
                sb.AppendLine($"{item.Naam}: {item.Aantal} keer");
            }
            sb.AppendLine();
            sb.AppendLine("=== GEMEENTEN (Top 10) ===");
            foreach (var gemeente in stats.GemeenteVerdeling)
            {
                sb.AppendLine($"{gemeente.GemeenteNaam}: {gemeente.AantalKlanten} klanten ({gemeente.Percentage:F1}%) - {gemeente.AantalStraten} straten");
            }

            File.WriteAllText(bestandspad, sb.ToString());
        }
    }
}
