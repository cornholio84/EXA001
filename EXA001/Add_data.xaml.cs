using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EXA001
{
    /// <summary>
    /// Interaction logic for Add_data.xaml
    /// </summary>
    public partial class Add_data : Window
    {
        private DatabaseHelper _dbHelper; // Instans av DatabaseHelper
        private Dictionary<string, List<string>> tableColumns = new Dictionary<string, List<string>>(); // Lagrar kolumnnamn för varje tabell

        public Add_data()
        {
            InitializeComponent();
            // Initiera DatabaseHelper med din anslutningssträng
            _dbHelper = new DatabaseHelper(
                "User ID=SA;" +
                "Password=SQL2022!;" +
                "Initial Catalog=YH_EXA001_PATMOL;" +
                "Data Source=192.168.1.72;" +
                "Integrated Security=false;" +
                "TrustServerCertificate=True;");
            LoadTableNames(); // Ladda tabellnamn vid start
        }

        // Laddar tabellnamn i ComboBox
        private void LoadTableNames()
        {
            List<string> tableNames = new List<string> { "Återförsäljare", "Kunder", "Produkt", "Orders" };
            TableComboBox.ItemsSource = tableNames;
        }

        // Händelsehanterare för när en ny tabell väljs
        private void TableComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TableComboBox.SelectedItem != null)
            {
                string selectedTable = TableComboBox.SelectedItem.ToString();
                GenerateInputFields(selectedTable); // Generera inmatningsfält för den valda tabellen
            }
        }

        // Genererar inmatningsfält för kolumnerna i den valda tabellen
        private void GenerateInputFields(string tableName)
        {
            FieldsPanel.Children.Clear(); // Rensa tidigare fält
            tableColumns.Clear(); // Rensa tidigare kolumninformation

            try
            {
                _dbHelper.OpenConnection(); // Öppna databasanslutning
                string query = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}'";
                SqlCommand command = new SqlCommand(query, _dbHelper.CurrentConnection); // Skapa ett SqlCommand-objekt
                SqlDataReader reader = command.ExecuteReader(); // Utför SQL-frågan

                // Läs kolumnnamn och skapa inmatningsfält för varje kolumn
                List<string> columns = new List<string>();
                while (reader.Read())
                {
                    string columnName = reader["COLUMN_NAME"].ToString();
                    columns.Add(columnName);

                    // Skapa en etikett och en textruta för varje kolumn
                    Label label = new Label { Content = columnName };
                    TextBox textBox = new TextBox { Name = columnName, Width = 200 };

                    FieldsPanel.Children.Add(label);
                    FieldsPanel.Children.Add(textBox);
                }
                tableColumns[tableName] = columns; // Lägg till kolumninformation i ordboken
                reader.Close(); // Stäng läsaren
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading columns: " + ex.Message); // Hantera eventuella fel
            }
            finally
            {
                _dbHelper.CloseConnection(); // Stäng databasanslutningen
            }
        }

        // Händelsehanterare för när användaren klickar på "Insert"-knappen
        private void InsertButton_Click(object sender, RoutedEventArgs e)
        {
            if (TableComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a table.");
                return;
            }

            string selectedTable = TableComboBox.SelectedItem.ToString(); // Hämta den valda tabellen
            Dictionary<string, string> columnData = new Dictionary<string, string>(); // Lagrar kolumnnamn och deras värden

            // Hämta data från inmatningsfälten
            foreach (var column in tableColumns[selectedTable])
            {
                TextBox textBox = FieldsPanel.Children.OfType<TextBox>().FirstOrDefault(tb => tb.Name == column);
                if (textBox != null)
                {
                    columnData[column] = textBox.Text; // Lägg till kolumnnamn och värde i ordboken
                }
            }

            InsertData(selectedTable, columnData); // Anropa metoden för att infoga data
        }

        // Infogar data i den valda tabellen
        private void InsertData(string tableName, Dictionary<string, string> columnData)
        {
            try
            {
                _dbHelper.OpenConnection(); // Öppna databasanslutning

                // Bygg dynamiskt upp INSERT SQL-kommandot
                string columns = string.Join(", ", columnData.Keys);
                string values = string.Join(", ", columnData.Values.Select(v => $"'{v}'"));
                string query = $"INSERT INTO {tableName} ({columns}) VALUES ({values})";

                SqlCommand command = new SqlCommand(query, _dbHelper.CurrentConnection); // Skapa ett SqlCommand-objekt

                command.ExecuteNonQuery(); // Utför SQL-kommandot
                MessageBox.Show("Data inserted successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inserting data: " + ex.Message); // Hantera eventuella fel
            }
            finally
            {
                _dbHelper.CloseConnection(); // Stäng databasanslutningen
            }
        }
    }
}
