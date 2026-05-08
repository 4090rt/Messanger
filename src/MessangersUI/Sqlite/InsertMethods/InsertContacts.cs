using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangersUI.Sqlite.InsertMethods
{
    public class InsertContacts
    {
        public ILogger<InsertContacts> _logger;
        public PoolSQLite _poolSQLite;

        public InsertContacts (ILogger<InsertContacts> logger, PoolSQLite poolSQLite)
        {
            _logger = logger;
            _poolSQLite = poolSQLite;
            
        }

        public async Task SaveContact(string username, DateTime dateTime, string photo)
        {
            MessageBox.Show("В методе");
            SQLiteConnection connection = null;
            SQLiteTransaction sqliteTransaction = null;
            try
            {
                connection = _poolSQLite.ConnctionOpen();
                MessageBox.Show("Начинаю выполнение");
                await using (sqliteTransaction = connection.BeginTransaction())
                {
                    string command = "INSERT INTO [ContactsBase] (Login, Date, PHOTO) VALUES (@L, @D, @P)";

                    await using (var sqlcommand = new SQLiteCommand(command, connection, sqliteTransaction))
                    {
                        sqlcommand.Parameters.AddWithValue("@L", username);
                        sqlcommand.Parameters.AddWithValue("@D", dateTime);
                        sqlcommand.Parameters.AddWithValue("@P", photo);

                        int rows = await sqlcommand.ExecuteNonQueryAsync();

                        if (rows > 0)
                        {
                            MessageBox.Show("Сохранено");
                        }
                        else
                        {
                            MessageBox.Show("НЕ Сохранено");
                        }
                    }
                    await sqliteTransaction.CommitAsync().ConfigureAwait(false);
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Возникло sql исключение" + ex.Message + ex.StackTrace);
                _logger.LogError("Возникло sql исключение" + ex.Message + ex.StackTrace);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Возникло sql исключение" + ex.Message + ex.StackTrace);
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace);
            }
            finally
            {
                if (sqliteTransaction != null)
                { 
                    sqliteTransaction.Dispose();
                }
                if (connection != null)
                { 
                    _poolSQLite.ConnectionClose(connection);
                }
            }
        }
    }
}
