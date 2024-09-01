using System;
using System.Data;
using System.Data.SqlClient;

namespace EXA001
{
    // Klass som hanterar databasanslutningar
    public class DatabaseHelper : IDisposable
    {
        private string _connectionString; // Anslutningssträng för databasen
        private SqlConnection _sqlConnection; // Objekt för databasanslutning
        private bool _disposed = false; // Flagga för att hålla reda på om objektet redan är frisläppt

        // Publik egenskap som ger tillgång till den aktuella SqlConnection
        public SqlConnection CurrentConnection => _sqlConnection;

        // Konstruktor som initierar med en anslutningssträng
        public DatabaseHelper(string connectionString)
        {
            _connectionString = connectionString; // Sätter anslutningssträngen
            _sqlConnection = new SqlConnection(_connectionString); // Skapar en ny SqlConnection
        }

        // Metod för att ändra anslutningssträng och skapa en ny SqlConnection
        public void SetConnectionString(string newConnectionString)
        {
            _connectionString = newConnectionString; // Uppdaterar anslutningssträngen
            _sqlConnection?.Dispose(); // Stänger och frigör befintlig anslutning om den existerar
            _sqlConnection = new SqlConnection(_connectionString); // Skapar en ny SqlConnection med den nya anslutningssträngen
        }

        // Öppnar anslutningen till databasen om den är stängd
        public void OpenConnection()
        {
            if (_sqlConnection.State == ConnectionState.Closed) // Kontrollerar om anslutningen är stängd
            {
                _sqlConnection.Open(); // Öppnar anslutningen
            }
        }

        // Stänger anslutningen till databasen om den är öppen
        public void CloseConnection()
        {
            if (_sqlConnection.State == ConnectionState.Open) // Kontrollerar om anslutningen är öppen
            {
                _sqlConnection.Close(); // Stänger anslutningen
            }
        }

        // Ger status på anslutningen (öppen/stängd/annat)
        public ConnectionState GetConnectionState()
        {
            return _sqlConnection.State; // Returnerar nuvarande status för anslutningen
        }
        // Metod för att exekvera SELECT-frågor och returnera resultat som DataTable
        public DataTable ExecuteQuery(string query)
        {
            DataTable dataTable = new DataTable();

            try
            {
                OpenConnection();
                using (SqlCommand command = new SqlCommand(query, _sqlConnection))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Kunde inte exekvera frågan", ex);
            }
            finally
            {
                CloseConnection();
            }

            return dataTable;
        }

        // Metod för att exekvera INSERT, UPDATE, DELETE, etc.
        public void ExecuteNonQuery(string query)
        {
            try
            {
                OpenConnection();
                using (SqlCommand command = new SqlCommand(query, _sqlConnection))
                {
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Kunde inte exekvera kommandot", ex);
            }
            finally
            {
                CloseConnection();
            }
        }
    

        // Implementerar IDisposable för att frigöra resurser korrekt
        public void Dispose()
        {
            Dispose(true); // Frigör resurser
            GC.SuppressFinalize(this); // Förhindrar att garbage collectorn anropar finalisern
        }

        // Metod för att frigöra resurser
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed) // Kontrollerar om objektet redan är frisläppt
            {
                if (disposing)
                {
                    _sqlConnection?.Dispose(); // Frigör SqlConnection om den är i bruk
                }
                _disposed = true; // Sätter flaggan för att indikera att objektet har frisläppts
            }
        }
    }
}
