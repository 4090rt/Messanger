using Messangers.Delegate;
using Messangers.SQLite.CreateDataBases;
using MessangersUI.Delegate;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangersUI.Sqlite.CreateTable
{
    public partial class  Create
    {
        private bool? _isCheckedCreate = false;
        private readonly ILogger<Create> _logger;
        private readonly PoolSQLite _poolSQLiteConnection;
        private readonly ExceptionDelegate _exceptionDelegate;

        public Create(ILogger<Create> logger, PoolSQLite poolSQLiteConnection, ExceptionDelegate exceptionDelegate)
        {
            _logger = logger;
            _poolSQLiteConnection = poolSQLiteConnection;
            _exceptionDelegate = exceptionDelegate;
        }
        public async Task Proverka()
        {
            if (_isCheckedCreate == true) return;

            if (_isCheckedCreate == false)
            { 
                await CreateTableUserContacts().ConfigureAwait(false);
            }

            _isCheckedCreate = true;
        }

        public async Task<bool> CreateTableUserContacts()
        {
            SQLiteConnection connection = null;
            try
            {
                connection = _poolSQLiteConnection.ConnctionOpen();

                string command = "CREATE TABLE IF NOT EXISTS ContactsBase (" +
                    "Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    "User TEXT NOT NULL," +
                    "Login TEXT NOT NULL, " +
                    "Date TEXT NOT NULL, " +
                    "PHOTO TEXT)";

                await using (var sqlcommand = new SQLiteCommand(command, connection))
                {
                    await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                    return true;
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show($"SQLite ошибка: {ex.Message}");
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return false;
            }
            finally
            {
                if (connection != null)
                {
                    _poolSQLiteConnection.ConnectionClose(connection);
                }
            }
        }
    }
}
