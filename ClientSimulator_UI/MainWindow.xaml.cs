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
using ClientSimulatorUtils.Services;
using Microsoft.Win32;

namespace ClientSimulator_UI
{
    public partial class MainWindow : Window
    {
        // Services (Business Layer)
        private readonly LandService _landService;
        private readonly SimulatieService _simulatieService;
        private readonly ExportService _exportService;

        // Data collections
        private ObservableCollection<Gemeente> _beschikbareGemeenten;
        private List<Persoon> _gegenereerdePersonen;
        private SimulatieStatistieken _huidigeStatistieken;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize services (business layer)
            _landService = new LandService();
            _simulatieService = new SimulatieService();
            _exportService = new ExportService();

            _beschikbareGemeenten = new ObservableCollection<Gemeente>();
            _gegenereerdePersonen = new List<Persoon>();
            _huidigeStatistieken = new SimulatieStatistieken();

            Loaded += MainWindow_Loaded;
        }

        // -----------------------------------------
        // INITIALISATIE
        // -----------------------------------------
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Vul dropdowns voor simulatie tab
                var landen = _landService.GetAllLanden();
                SimLandComboBox.ItemsSource = landen;
                ZoekLandComboBox.ItemsSource = landen;

                // Stel defaults in
                SimulatieProgressBar.Value = 0;
                GemeenteFilterCheckBox.IsChecked = false;
                GemeenteListView.IsEnabled = false;

                UpdateSimulatieStatus("Klaar voor simulatie");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij initialisatie: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                var gemeenten = _simulatieService.GetGemeentenByLand(landId);
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
                    _gegenereerdePersonen = _simulatieService.VoerSimulatieUit(landId, aantalKlanten, minLeeftijd, maxLeeftijd, opdrachtgever);

                    // Genereer huisnummers
                    foreach (var persoon in _gegenereerdePersonen)
                    {
                        persoon.Huisnummer = GenereerHuisnummer();
                    }

                    // Update progress
                    Dispatcher.Invoke(() => SimulatieProgressBar.Value = 100);
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

            // Gebruik service voor statistieken
            _huidigeStatistieken = _simulatieService.BerekenStatistieken(_gegenereerdePersonen);

            var sb = new StringBuilder();
            sb.AppendLine("=== DATASET STATISTIEKEN ===");
            sb.AppendLine($"Dataset ID: 1");
            sb.AppendLine($"Land: {SimLandComboBox.Text}");
            sb.AppendLine($"Opdrachtgever: {OpdrachtgeverTextBox.Text}");
            sb.AppendLine($"Aanmaak datum: {DateTime.Now:dd-MM-yyyy HH:mm:ss}");
            sb.AppendLine();

            sb.AppendLine("=== KLANTEN OVERZICHT ===");
            sb.AppendLine($"Totaal aantal klanten: {_huidigeStatistieken.TotaalKlanten}");
            sb.AppendLine();

            sb.AppendLine("=== LEEFTIJD ANALYSE ===");
            sb.AppendLine($"Gemiddelde leeftijd bij simulatie: {_huidigeStatistieken.GemiddeldeLeeftijd:F1} jaar");
            sb.AppendLine($"Minimum leeftijd: {_huidigeStatistieken.MinimumLeeftijd} jaar");
            sb.AppendLine($"Maximum leeftijd: {_huidigeStatistieken.MaximumLeeftijd} jaar");
            if (_huidigeStatistieken.JongsteKlant != null)
                sb.AppendLine($"Jongste klant: {_huidigeStatistieken.JongsteKlant.Voornaam} {_huidigeStatistieken.JongsteKlant.Achternaam} ({_huidigeStatistieken.JongsteKlant.Leeftijd} jaar)");
            if (_huidigeStatistieken.OudsteKlant != null)
                sb.AppendLine($"Oudste klant: {_huidigeStatistieken.OudsteKlant.Voornaam} {_huidigeStatistieken.OudsteKlant.Achternaam} ({_huidigeStatistieken.OudsteKlant.Leeftijd} jaar)");
            sb.AppendLine();

            sb.AppendLine("=== VOORNAMEN (Top 10) ===");
            foreach (var item in _huidigeStatistieken.TopVoornamen)
            {
                sb.AppendLine($"{item.Naam}: {item.Aantal} keer");
            }
            sb.AppendLine();

            sb.AppendLine("=== ACHTERNAMEN (Top 10) ===");
            foreach (var item in _huidigeStatistieken.TopAchternamen)
            {
                sb.AppendLine($"{item.Naam}: {item.Aantal} keer");
            }
            sb.AppendLine();

            sb.AppendLine("=== GEMEENTEN (Top 10) ===");
            foreach (var gemeente in _huidigeStatistieken.GemeenteVerdeling)
            {
                sb.AppendLine($"{gemeente.GemeenteNaam}: {gemeente.AantalKlanten} klanten ({gemeente.Percentage:F1}%)");
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
                ExporteerDataset(format, separator, bestandsnaam, _huidigeStatistieken);

                MessageBox.Show("Export voltooid!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout tijdens export: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExporteerDataset(string format, string separator, string bestandsnaam, SimulatieStatistieken stats)
        {
            string exportDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exports");
            Directory.CreateDirectory(exportDir);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            if (format == "json")
            {
                // Export als JSON
                string jsonPath = Path.Combine(exportDir, $"{bestandsnaam}_{timestamp}.json");
                _exportService.ExporteerNaarJson(_gegenereerdePersonen, stats, jsonPath);
                ExportLogTextBox.Text = $"JSON bestand geëxporteerd: {jsonPath}\n" + ExportLogTextBox.Text;
            }
            else if (format == "combined_text")
            {
                // Export als 1 tekstbestand (data + statistieken)
                string txtPath = Path.Combine(exportDir, $"{bestandsnaam}_{timestamp}.txt");
                _exportService.ExporteerNaarCsv(_gegenereerdePersonen, stats, txtPath, separator);

                // Voeg statistieken toe aan hetzelfde bestand
                string statsContent = "\n\n" + StatistiekenTextBox.Text;
                File.AppendAllText(txtPath, statsContent);

                ExportLogTextBox.Text = $"Tekstbestand geëxporteerd: {txtPath}\n" + ExportLogTextBox.Text;
            }
            else if (format == "separate_text")
            {
                // Export als 2 tekstbestanden (apart)
                string dataPath = Path.Combine(exportDir, $"{bestandsnaam}_data_{timestamp}.txt");
                string statsPath = Path.Combine(exportDir, $"{bestandsnaam}_stats_{timestamp}.txt");

                // Data bestand
                _exportService.ExporteerNaarCsv(_gegenereerdePersonen, stats, dataPath, separator);

                // Statistieken bestand
                _exportService.ExporteerStatistieken(stats, statsPath);

                ExportLogTextBox.Text = $"Data bestand: {dataPath}\nStatistieken bestand: {statsPath}\n" + ExportLogTextBox.Text;
            }
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
