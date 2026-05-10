using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangersUI.Sqlite.DeleteUser
{
    public class Delete
    {
        private ILogger<Delete> _logger;
        private PoolSQLite _poolsqlite;


        public Delete(ILogger<Delete> logger, PoolSQLite poolsqlite)
        {
            _logger = logger;
            _poolsqlite = poolsqlite;
        }

        public async Task DeleteMethod(string username, string login)
        { 
            SQLiteConnection connection = null;
            SQLiteTransaction transaction = null;
            try
            {
                connection = _poolsqlite.ConnctionOpen();
                transaction = connection.BeginTransaction();
                    string command = "DELETE FROM ContactsBase WHERE Login = @L";

                    await using (var sqlitecommand = new SQLiteCommand(command, connection, transaction))
                    { 
                        sqlitecommand.Parameters.AddWithValue("@L", login);

                        await sqlitecommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }
                await transaction.CommitAsync();
            }
            catch (SQLiteException ex)
            {
                _logger.LogError("Возникло SQLite исключение" + ex.Message + ex.StackTrace + ex.InnerException);
                if (transaction != null) await transaction.RollbackAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError("Возникло  исключение" + ex.Message + ex.StackTrace + ex.InnerException);
                if (transaction != null) await transaction.RollbackAsync();
            }
            finally
            {
                if (connection != null)
                { 
                    _poolsqlite.ConnectionClose(connection);
                }
                if (transaction != null)
                {
                    transaction.Dispose();
                }
            }
        }
    }
}
