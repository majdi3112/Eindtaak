using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClientSimulator_BL.Manager;
using ClientSimulator_BL.Model;
using ClientSimulator_DL.Repository;
using Microsoft.Win32;

namespace ClientSimulator_UI
{
    public partial class MainWindow : Window
    {
        // Managers (Business Layer)
        private readonly PersoonManager _persoonMgr;
        private readonly VoornaamManager _voornaamMgr;
        private readonly AchternaamManager _achternaamMgr;
        private readonly GemeenteManager _gemeenteMgr;
        private readonly StraatManager _straatMgr;

        // Repositories (Data Layer)
        private readonly LandRepository _landRepo;
        private readonly GemeenteRepository _gemeenteRepo;

        // Data collections
        private ObservableCollection<Gemeente> _beschikbareGemeenten;
        private List<Persoon> _gegenereerdePersonen;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize managers (business layer)
            var voornaamRepo = new VoornaamRepository();
            var achternaamRepo = new AchternaamRepository();
            _voornaamMgr = new VoornaamManager(voornaamRepo);
            _achternaamMgr = new AchternaamManager(achternaamRepo);
            _gemeenteMgr = new GemeenteManager(new GemeenteRepository());
            _straatMgr = new StraatManager(new StraatRepository());
            _persoonMgr = new PersoonManager(_voornaamMgr, _achternaamMgr, _gemeenteMgr, _straatMgr);

            // Initialize repositories
            _landRepo = new LandRepository();
            _gemeenteRepo = new GemeenteRepository();

            _beschikbareGemeenten = new ObservableCollection<Gemeente>();
            _gegenereerdePersonen = new List<Persoon>();

            Loaded += MainWindow_Loaded;
        }

        // -----------------------------------------
        // INITIALISATIE
        // -----------------------------------------
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Vul dropdowns voor simulatie tab
            SimLandComboBox.ItemsSource = _landRepo.GetAll();
            ZoekLandComboBox.ItemsSource = _landRepo.GetAll();

            // Stel defaults in
            SimulatieProgressBar.Value = 0;
            GemeenteFilterCheckBox.IsChecked = false;
            GemeenteListView.IsEnabled = false;

            UpdateSimulatieStatus("Klaar voor simulatie");
        }

        private void UpdateSimulatieStatus(string message)
        {
            Dispatcher.Invoke(() =>
            {
                SimulatieResultatenTextBox.Text = $"[{DateTime.Now:HH:mm:ss}] {message}\n" + SimulatieResultatenTextBox.Text;
            });
        }

        // -----------------------------------------
        // SIMULATIE EVENT HANDLERS
        // -----------------------------------------
        private void GemeenteFilterCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = GemeenteFilterCheckBox.IsChecked ?? false;
            GemeenteListView.IsEnabled = isChecked;

            if (isChecked && SimLandComboBox.SelectedValue != null)
            {
                int landId = (int)SimLandComboBox.SelectedValue;
                var gemeenten = _gemeenteRepo.GetByLand(landId);
                GemeenteListView.ItemsSource = gemeenten;
            }
        }

        private async void StartSimulatieButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validatie
                if (SimLandComboBox.SelectedValue == null)
                {
                    MessageBox.Show("Selecteer eerst een land.", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(AantalKlantenTextBox.Text, out int aantalKlanten) || aantalKlanten <= 0)
                {
                    MessageBox.Show("Voer een geldig aantal klanten in (groter dan 0).", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(MinLeeftijdTextBox.Text, out int minLeeftijd) || minLeeftijd < 0)
                {
                    MessageBox.Show("Voer een geldige minimum leeftijd in.", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(MaxLeeftijdTextBox.Text, out int maxLeeftijd) || maxLeeftijd <= minLeeftijd)
                {
                    MessageBox.Show("Voer een geldige maximum leeftijd in (groter dan minimum).", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Simulatie parameters
                int landId = (int)SimLandComboBox.SelectedValue;
                string opdrachtgever = OpdrachtgeverTextBox.Text.Trim();

                if (string.IsNullOrEmpty(opdrachtgever))
                {
                    MessageBox.Show("Voer een opdrachtgever in.", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Disable UI tijdens simulatie
                StartSimulatieButton.IsEnabled = false;
                SimulatieProgressBar.Value = 0;

                UpdateSimulatieStatus($"Start simulatie voor {aantalKlanten} klanten in land {landId}...");

                // Start simulatie in background
                await Task.Run(() =>
                {
                    _gegenereerdePersonen.Clear();

                    for (int i = 0; i < aantalKlanten; i++)
                    {
                        var persoon = _persoonMgr.Genereer(landId, minLeeftijd, maxLeeftijd, opdrachtgever);
                        persoon.Huisnummer = GenereerHuisnummer();
                        _gegenereerdePersonen.Add(persoon);

                        // Update progress
                        int progress = (i + 1) * 100 / aantalKlanten;
                        Dispatcher.Invoke(() => SimulatieProgressBar.Value = progress);
                    }
                });

                UpdateSimulatieStatus($"Simulatie voltooid! {aantalKlanten} personen gegenereerd.");
                ToonSimulatieResultaten();

                // Re-enable UI
                StartSimulatieButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout tijdens simulatie: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                StartSimulatieButton.IsEnabled = true;
            }
        }

        private void ResetSimulatieButton_Click(object sender, RoutedEventArgs e)
        {
            SimLandComboBox.SelectedIndex = -1;
            AantalKlantenTextBox.Text = "100";
            OpdrachtgeverTextBox.Text = "Test Opdrachtgever";
            MinLeeftijdTextBox.Text = "18";
            MaxLeeftijdTextBox.Text = "90";
            GemeenteFilterCheckBox.IsChecked = false;
            SimulatieProgressBar.Value = 0;
            SimulatieResultatenTextBox.Text = "";
            _gegenereerdePersonen.Clear();

            UpdateSimulatieStatus("Formulier gereset");
        }

        private string GenereerHuisnummer()
        {
            if (!int.TryParse(MaxHuisnummerTextBox.Text, out int maxNummer))
                maxNummer = 999;

            if (!int.TryParse(PercentageLettersTextBox.Text, out int percLetters))
                percLetters = 10;

            Random rand = new Random();
            int nummer = rand.Next(1, maxNummer + 1);

            // Bepaal of letter toevoegen
            if (rand.Next(100) < percLetters)
            {
                char letter = (char)('A' + rand.Next(26));
                return $"{nummer}{letter}";
            }

            return nummer.ToString();
        }

        private void ToonSimulatieResultaten()
        {
            if (_gegenereerdePersonen.Count == 0) return;

            var sb = new StringBuilder();
            sb.AppendLine("=== SIMULATIE RESULTATEN ===");
            sb.AppendLine($"Totaal gegenereerd: {_gegenereerdePersonen.Count} personen");
            sb.AppendLine();

            // Toon eerste 5 personen als voorbeeld
            sb.AppendLine("Voorbeeld personen:");
            for (int i = 0; i < Math.Min(5, _gegenereerdePersonen.Count); i++)
            {
                var p = _gegenereerdePersonen[i];
                sb.AppendLine($"{i + 1}. {p.Voornaam} {p.Achternaam} ({p.Leeftijd} jaar)");
                sb.AppendLine($"   Adres: {p.Straat} {p.Huisnummer}, {p.Gemeente}");
                sb.AppendLine($"   Opdrachtgever: {p.Opdrachtgever}");
                sb.AppendLine();
            }

            // Basis statistieken
            var leeftijdGemiddelde = _gegenereerdePersonen.Average(p => p.Leeftijd);
            var minLeeftijd = _gegenereerdePersonen.Min(p => p.Leeftijd);
            var maxLeeftijd = _gegenereerdePersonen.Max(p => p.Leeftijd);

            sb.AppendLine("=== BASIS STATISTIEKEN ===");
            sb.AppendLine($"Gemiddelde leeftijd: {leeftijdGemiddelde:F1} jaar");
            sb.AppendLine($"Minimum leeftijd: {minLeeftijd} jaar");
            sb.AppendLine($"Maximum leeftijd: {maxLeeftijd} jaar");

            UpdateSimulatieStatus(sb.ToString());
        }

        // -----------------------------------------
        // DATASET BEHEER EVENT HANDLERS
        // -----------------------------------------
        private void ZoekDatasetsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int? landId = null;
                string opdrachtgever = null;

                if (ZoekLandComboBox.SelectedValue != null)
                    landId = (int)ZoekLandComboBox.SelectedValue;

                opdrachtgever = ZoekOpdrachtgeverTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(opdrachtgever))
                    opdrachtgever = null;

                // Voor nu tonen we alleen de gegenereerde dataset
                // TODO: Implementeer echte dataset opslag en ophaling uit database

                if (_gegenereerdePersonen.Count > 0)
                {
                    // Maak een dummy dataset object
                    var dataset = new
                    {
                        DatasetId = 1,
                        Land = SimLandComboBox.SelectedItem?.ToString() ?? "Onbekend",
                        Opdrachtgever = OpdrachtgeverTextBox.Text,
                        AanmaakDatum = DateTime.Now.ToString("dd-MM-yyyy HH:mm"),
                        TotaalKlanten = _gegenereerdePersonen.Count
                    };

                    DatasetsListView.ItemsSource = new[] { dataset };
                }
                else
                {
                    DatasetsListView.ItemsSource = null;
                    MessageBox.Show("Geen datasets gevonden. Voer eerst een simulatie uit.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij zoeken: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DatasetsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatasetsListView.SelectedItem == null) return;

            // Toon statistieken voor geselecteerde dataset
            ToonDatasetStatistieken();
        }

        private void ToonDatasetStatistieken()
        {
            if (_gegenereerdePersonen.Count == 0)
            {
                StatistiekenTextBox.Text = "Geen data beschikbaar";
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("=== DATASET STATISTIEKEN ===");
            sb.AppendLine($"Dataset ID: 1");
            sb.AppendLine($"Land: {SimLandComboBox.Text}");
            sb.AppendLine($"Opdrachtgever: {OpdrachtgeverTextBox.Text}");
            sb.AppendLine($"Aanmaak datum: {DateTime.Now:dd-MM-yyyy HH:mm:ss}");
            sb.AppendLine();

            sb.AppendLine("=== KLANTEN OVERZICHT ===");
            sb.AppendLine($"Totaal aantal klanten: {_gegenereerdePersonen.Count}");
            sb.AppendLine();

            // Leeftijd statistieken
            var leeftijdGemiddelde = _gegenereerdePersonen.Average(p => p.Leeftijd);
            var huidigeLeeftijdGemiddelde = _gegenereerdePersonen.Average(p => p.HuidigeLeeftijd);
            var minLeeftijd = _gegenereerdePersonen.Min(p => p.Leeftijd);
            var maxLeeftijd = _gegenereerdePersonen.Max(p => p.Leeftijd);

            sb.AppendLine("=== LEEFTIJD ANALYSE ===");
            sb.AppendLine($"Gemiddelde leeftijd bij simulatie: {leeftijdGemiddelde:F1} jaar");
            sb.AppendLine($"Gemiddelde huidige leeftijd: {huidigeLeeftijdGemiddelde:F1} jaar");
            sb.AppendLine($"Jongste klant: {minLeeftijd} jaar");
            sb.AppendLine($"Oudste klant: {maxLeeftijd} jaar");
            sb.AppendLine();

            // Naam frequenties
            var voornaamFrequenties = _gegenereerdePersonen
                .GroupBy(p => p.Voornaam)
                .OrderByDescending(g => g.Count())
                .Take(10);

            sb.AppendLine("=== VOORNAMEN (Top 10) ===");
            foreach (var group in voornaamFrequenties)
            {
                sb.AppendLine($"{group.Key}: {group.Count()} keer");
            }
            sb.AppendLine();

            var achternaamFrequenties = _gegenereerdePersonen
                .GroupBy(p => p.Achternaam)
                .OrderByDescending(g => g.Count())
                .Take(10);

            sb.AppendLine("=== ACHTERNAMEN (Top 10) ===");
            foreach (var group in achternaamFrequenties)
            {
                sb.AppendLine($"{group.Key}: {group.Count()} keer");
            }
            sb.AppendLine();

            // Gemeente verdeling
            var gemeenteVerdeling = _gegenereerdePersonen
                .GroupBy(p => p.Gemeente)
                .OrderByDescending(g => g.Count())
                .Take(10);

            sb.AppendLine("=== GEMEENTEN (Top 10) ===");
            foreach (var group in gemeenteVerdeling)
            {
                double percentage = (double)group.Count() / _gegenereerdePersonen.Count * 100;
                sb.AppendLine($"{group.Key}: {group.Count()} klanten ({percentage:F1}%)");
            }

            StatistiekenTextBox.Text = sb.ToString();
        }

        // -----------------------------------------
        // EXPORT EVENT HANDLERS
        // -----------------------------------------
        private void ExportDatasetButton_Click(object sender, RoutedEventArgs e)
        {
            if (_gegenereerdePersonen.Count == 0)
            {
                MessageBox.Show("Geen data om te exporteren. Voer eerst een simulatie uit.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // Bepaal export formaat
                string format;
                if (ExportFormat1Radio.IsChecked == true)
                    format = "combined_text";
                else if (ExportFormat2Radio.IsChecked == true)
                    format = "separate_text";
                else
                    format = "json";

                // Bepaal scheidingsteken
                string separator = ";"; // default
                switch (ScheidingstekenComboBox.SelectedIndex)
                {
                    case 0: separator = ";"; break;
                    case 1: separator = ","; break;
                    case 2: separator = "\t"; break;
                    case 3: separator = "|"; break;
                }

                string bestandsnaam = ExportBestandsnaamTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(bestandsnaam))
                    bestandsnaam = "export_dataset";

                // Export uitvoeren
                ExporteerDataset(format, separator, bestandsnaam);

                MessageBox.Show("Export voltooid!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout tijdens export: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExporteerDataset(string format, string separator, string bestandsnaam)
        {
            string exportDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exports");
            Directory.CreateDirectory(exportDir);

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            if (format == "json")
            {
                // Export als JSON
                string jsonPath = Path.Combine(exportDir, $"{bestandsnaam}_{timestamp}.json");

                var exportData = new
                {
                    metadata = new
                    {
                        exportDate = DateTime.Now,
                        totalCustomers = _gegenereerdePersonen.Count,
                        country = SimLandComboBox.Text,
                        customer = OpdrachtgeverTextBox.Text
                    },
                    customers = _gegenereerdePersonen.Select(p => new
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
                File.WriteAllText(jsonPath, json);

                ExportLogTextBox.Text = $"JSON bestand geëxporteerd: {jsonPath}\n" + ExportLogTextBox.Text;
            }
            else if (format == "combined_text")
            {
                // Export als 1 tekstbestand (data + statistieken)
                string txtPath = Path.Combine(exportDir, $"{bestandsnaam}_{timestamp}.txt");
                var content = GenereerExportContent(separator, includeStats: true);
                File.WriteAllText(txtPath, content);

                ExportLogTextBox.Text = $"Tekstbestand geëxporteerd: {txtPath}\n" + ExportLogTextBox.Text;
            }
            else if (format == "separate_text")
            {
                // Export als 2 tekstbestanden (apart)
                string dataPath = Path.Combine(exportDir, $"{bestandsnaam}_data_{timestamp}.txt");
                string statsPath = Path.Combine(exportDir, $"{bestandsnaam}_stats_{timestamp}.txt");

                // Data bestand
                var dataContent = GenereerExportContent(separator, includeStats: false);
                File.WriteAllText(dataPath, dataContent);

                // Statistieken bestand
                var statsContent = StatistiekenTextBox.Text;
                File.WriteAllText(statsPath, statsContent);

                ExportLogTextBox.Text = $"Data bestand: {dataPath}\nStatistieken bestand: {statsPath}\n" + ExportLogTextBox.Text;
            }
        }

        private string GenereerExportContent(string separator, bool includeStats)
        {
            var sb = new StringBuilder();

            if (includeStats)
            {
                sb.AppendLine("=== EXPORT METADATA ===");
                sb.AppendLine($"Export datum: {DateTime.Now:dd-MM-yyyy HH:mm:ss}");
                sb.AppendLine($"Land: {SimLandComboBox.Text}");
                sb.AppendLine($"Opdrachtgever: {OpdrachtgeverTextBox.Text}");
                sb.AppendLine($"Totaal klanten: {_gegenereerdePersonen.Count}");
                sb.AppendLine();
                sb.AppendLine("=== KLANTEN DATA ===");
            }

            // CSV header
            sb.AppendLine($"Voornaam{separator}Achternaam{separator}Geslacht{separator}Leeftijd{separator}Straat{separator}Huisnummer{separator}Gemeente{separator}Land{separator}Opdrachtgever{separator}Geboortedatum{separator}HuidigeLeeftijd");

            // Data rijen
            foreach (var persoon in _gegenereerdePersonen)
            {
                sb.AppendLine($"{persoon.Voornaam}{separator}{persoon.Achternaam}{separator}{persoon.Geslacht}{separator}{persoon.Leeftijd}{separator}{persoon.Straat}{separator}{persoon.Huisnummer}{separator}{persoon.Gemeente}{separator}{persoon.Land}{separator}{persoon.Opdrachtgever}{separator}{persoon.GeboorteDatum:dd-MM-yyyy}{separator}{persoon.HuidigeLeeftijd}");
            }

            if (includeStats)
            {
                sb.AppendLine();
                sb.AppendLine("=== STATISTIEKEN ===");
                sb.AppendLine(StatistiekenTextBox.Text);
            }

            return sb.ToString();
        }

        private void OpenExportMapButton_Click(object sender, RoutedEventArgs e)
        {
            string exportDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exports");

            if (Directory.Exists(exportDir))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = exportDir,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            else
            {
                MessageBox.Show("Export map bestaat nog niet. Exporteer eerst een dataset.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
