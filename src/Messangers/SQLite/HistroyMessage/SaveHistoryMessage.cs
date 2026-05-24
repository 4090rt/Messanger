using Messangers.Delegate;
using Messangers.SQLite.ContactBse.UserSave;
using Messangers.SQLite.PoolSQLiteConnection;
using MessangersUI.Delegate;
using System.Data.SQLite;

namespace Messangers.SQLite.HistroyMessage
{
    public class SaveHistoryMessage
    {
        private readonly ILogger<SaveHistoryMessage> _logger;
        private readonly PoolSQLite _poolSQLiteConnection;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly ExceptionDelegate _exceptionDelegate;

        public SaveHistoryMessage(ILogger<SaveHistoryMessage> logger, PoolSQLite poolSQLiteConnection, SQLiteExceptionDelegate sQLiteExceptionDelegate, ExceptionDelegate exceptionDelegate)
        {
            _logger = logger;
            _poolSQLiteConnection = poolSQLiteConnection;
            _sQLiteExceptionDelegate = sQLiteExceptionDelegate;
            _exceptionDelegate = exceptionDelegate;
        }

        public async Task SaveMethod(string user1, string user2, string message, string datetime, string state)
        {
            SQLiteConnection connection = null;
            SQLiteTransaction transaction = null;
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();
                transaction = connection.BeginTransaction();

                string command = "INSERT INTO [HistoryMessage] (LoginUser1, LoginUser2, Message, Date, State) VALUES (@U1, @U2, @M, @D, @S)";

                await using (var sqlcommand = new SQLiteCommand(command, connection, transaction))
                {
                    sqlcommand.Parameters.AddWithValue("@U1", user1);
                    sqlcommand.Parameters.AddWithValue("@U2", user2);
                    sqlcommand.Parameters.AddWithValue("@M", message);
                    sqlcommand.Parameters.AddWithValue("@D", datetime);
                    sqlcommand.Parameters.AddWithValue("@S",state);

                    int rows = await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                    if (rows > 0)
                    {
                        _logger.LogInformation($"Информация о контакте сохранена. Затронуто строк: {rows}");
                    }
                    else
                    {
                        _logger.LogWarning($"Информация о контакте не сохранена. Затронуто строк: {rows}");
                    }
                }
                await transaction.CommitAsync().ConfigureAwait(false);

            }
            catch (SQLiteException ex)
            {
                await _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex);
                await (transaction?.RollbackAsync() ?? Task.CompletedTask);
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                await (transaction?.RollbackAsync() ?? Task.CompletedTask);
            }
            finally
            {
                if (connection != null)
                {
                    _poolSQLiteConnection.CloseConnection(connection);
                }
                if (transaction != null)
                {
                    transaction.Dispose();
                }
            }
        }
    }
}
