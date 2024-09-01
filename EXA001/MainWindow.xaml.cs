using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace EXA001
{
    public partial class MainWindow : Window
    {
        private DatabaseHelper _dbHelper;
        private DispatcherTimer _timer;


        public MainWindow()
        {
            InitializeComponent();

            // Initiera DatabaseHelper med anslutningssträngen
            _dbHelper = new DatabaseHelper(
                "User ID=SA;" +
                "Password=SQL2022!;" +
                "Initial Catalog=YH_EXA001_PATMOL;" +
                "Data Source=192.168.1.72;" +
                "Integrated Security=false;" +
                "TrustServerCertificate=True;");

            SetupTimer(); // Starta en timer för att övervaka anslutningsstatus

            // Ladda data vid klick på respektive knapp
            Kunder.Click += (s, e) => LoadData("SELECT * FROM Kunder");
            Återförsäljare.Click += (s, e) => LoadData("SELECT * FROM Återförsäljare");
            Produkter.Click += (s, e) => LoadData("SELECT * FROM Produkt");
            Ordrar.Click += (s, e) => LoadData("SELECT * FROM Orders");

            // Fyll ComboBoxar med alternativ
            Window_Loaded(null, null);
        }

        private void OpenAdd_data(object sender, RoutedEventArgs e)
        {
            Add_data addDataWindow = new Add_data();
            addDataWindow.Show();
        }

        private void SetupTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(5);
            _timer.Tick += (s, e) => CheckAndRefreshConnectionStatus();
            _timer.Start();
        }

        private void CheckAndRefreshConnectionStatus()
        {
            try
            {
                _dbHelper.OpenConnection(); // Försök att öppna anslutningen
                UpdateConnectionStatusLamp(true);
            }
            catch (SqlException)
            {
                UpdateConnectionStatusLamp(false);
            }
            finally
            {
                _dbHelper.CloseConnection(); // Stäng anslutningen om den är öppen
            }
        }

        private void UpdateConnectionStatusLamp(bool isConnected)
        {
            ConnectionStatusLamp.Fill = isConnected ? Brushes.Green : Brushes.Red;
        }

        private void LoadData(string query)
        {
            try
            {
                DataTable dataTable = _dbHelper.ExecuteQuery(query); // Använd DatabaseHelper för att exekvera SQL-frågan
                if (dataTable.Rows.Count > 0)
                {
                    QueryDataGrid.ItemsSource = dataTable.DefaultView;
                }
                else
                {
                    MessageBox.Show("Ingen data hittades.");
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Kunde inte ladda data: {ex.Message}");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FillComboBoxes(); // Fyll ComboBoxar med alternativ vid fönstrets laddning
        }

        private void FillComboBoxes()
        {
            KunderComboBox.ItemsSource = new List<string>
            {
                "Kunder registrerade efter ett specifikt datum",
                "Kundens namn och email",
                "Kunder i en specifik stad",
                "Kunder från ett specifikt land",
                "Kunder med ett specifikt postnummer"
            };

            ÅterförsäljareComboBox.ItemsSource = new List<string>
            {
                "Återförsäljare i en specifik stad",
                "Kontaktinformation för alla återförsäljare",
                "Återförsäljare med specifikt organisationsnummer",
                "Återförsäljare i ett specifikt land",
                "Återförsäljare med specifik webbadress"
            };

            ProdukterComboBox.ItemsSource = new List<string>
            {
                "Produkter i en specifik kategori",
                "Produkter med specifik rabatt",
                "Produkter från en specifik återförsäljare",
                "Produkter med specifikt lagerantal",
                "Produkter skapade efter ett specifikt datum"
            };

            OrdrarComboBox.ItemsSource = new List<string>
            {
                "Ordrar med specifik status",
                "Ordrar skapade efter ett specifikt datum",
                "Ordrar för en specifik kund",
                "Ordrar med specifik betalningsmetod",
                "Ordrar med totalt belopp över ett visst värde"
            };
        }

        // Hantera val i ComboBoxar på ett generiskt sätt
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            string selectedFilter = comboBox.SelectedItem as string;
            string query = GetQueryBasedOnSelection(selectedFilter, comboBox.Name); // Hämta SQL-frågan baserat på valet

            if (!string.IsNullOrEmpty(query))
            {
                LoadData(query); // Anropa LoadData med den genererade frågan
            }
        }

        private string GetQueryBasedOnSelection(string selectedFilter, string comboBoxName)
        {
            // Definiera SQL-frågor baserat på ComboBoxens namn och det valda filtret
            if (comboBoxName == "KunderComboBox")
            {
                return selectedFilter switch
                {
                    "Kunder registrerade efter ett specifikt datum" => "SELECT * FROM Kunder WHERE Registreringsdatum > '2023-01-01'",
                    "Kundens namn och email" => "SELECT Namn, Email FROM Kunder",
                    "Kunder i en specifik stad" => "SELECT * FROM Kunder WHERE Stad = 'Göteborg'",
                    "Kunder från ett specifikt land" => "SELECT * FROM Kunder WHERE Land = 'Norge'",
                    "Kunder med ett specifikt postnummer" => "SELECT * FROM Kunder WHERE Postnummer = '12345'",
                    _ => string.Empty
                };
            }
            else if (comboBoxName == "ÅterförsäljareComboBox")
            {
                return selectedFilter switch
                {
                    "Återförsäljare i en specifik stad" => "SELECT * FROM Återförsäljare WHERE Stad = 'Stockholm'",
                    "Kontaktinformation för alla återförsäljare" => "SELECT Namn, Kontaktperson, Telefonnummer, Email FROM Återförsäljare",
                    "Återförsäljare med specifikt organisationsnummer" => "SELECT * FROM Återförsäljare WHERE Organisationsnummer = '123456-7890'",
                    "Återförsäljare i ett specifikt land" => "SELECT * FROM Återförsäljare WHERE Land = 'Sverige'",
                    "Återförsäljare med specifik webbadress" => "SELECT * FROM Återförsäljare WHERE Webbadress = 'www.example.com'",
                    _ => string.Empty
                };
            }
            else if (comboBoxName == "ProdukterComboBox")
            {
                return selectedFilter switch
                {
                    "Produkter i en specifik kategori" => "SELECT * FROM Produkt WHERE KategoriID = 1",
                    "Produkter med specifik rabatt" => "SELECT * FROM Produkt WHERE Rabatt > 10.00",
                    "Produkter från en specifik återförsäljare" => "SELECT * FROM Produkt WHERE ÅterförsäljareID = 2",
                    "Produkter med specifikt lagerantal" => "SELECT * FROM Produkt WHERE LagerAntal < 50",
                    "Produkter skapade efter ett specifikt datum" => "SELECT * FROM Produkt WHERE SkapadDatum > '2023-01-01'",
                    _ => string.Empty
                };
            }
            else if (comboBoxName == "OrdrarComboBox")
            {
                return selectedFilter switch
                {
                    "Ordrar med specifik status" => "SELECT * FROM Orders WHERE Orderstatus = 'Pending'",
                    "Ordrar skapade efter ett specifikt datum" => "SELECT * FROM Orders WHERE Beställningsdatum > '2023-01-01'",
                    "Ordrar för en specifik kund" => "SELECT * FROM Orders WHERE KundID = 3",
                    "Ordrar med specifik betalningsmetod" => "SELECT * FROM Orders WHERE Betalningsmetod = 'Kreditkort'",
                    "Ordrar med totalt belopp över ett visst värde" => "SELECT * FROM Orders WHERE TotalBelopp > 1000.00",
                    _ => string.Empty
                };
            }

            return string.Empty;
        }

        protected override void OnClosed(EventArgs e)
        {
            _dbHelper.CloseConnection(); // Stäng anslutningen när fönstret stängs
            base.OnClosed(e);
        }
    }
}
