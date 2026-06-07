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

        public async Task<long> SaveMethod(string user1, string user2, string message, string datetime, string state)
        {
            SQLiteConnection connection = null;
            SQLiteTransaction transaction = null;
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();
                transaction = connection.BeginTransaction();

                string command = "INSERT INTO [HistoryMessage] (LoginUser1, LoginUser2, Message, Date, State) VALUES (@U1, @U2, @M, @D, @S); SELECT last_insert_rowid();";

                await using (var sqlcommand = new SQLiteCommand(command, connection, transaction))
                {
                    sqlcommand.Parameters.AddWithValue("@U1", user1);
                    sqlcommand.Parameters.AddWithValue("@U2", user2);
                    sqlcommand.Parameters.AddWithValue("@M", message);
                    sqlcommand.Parameters.AddWithValue("@D", datetime);
                    sqlcommand.Parameters.AddWithValue("@S",state);

                    long insertedId = (long)await sqlcommand.ExecuteScalarAsync().ConfigureAwait(false);
                    if (insertedId > 0)
                    {
                        await transaction.CommitAsync().ConfigureAwait(false);
                        _logger.LogInformation($"Информация о контакте сохранена. ID: {insertedId}, Затронуто строк: 1");
                        return insertedId;
                    }
                    else
                    {
                        _logger.LogWarning($"Информация о контакте не сохранена. Затронуто строк: {insertedId}");
                        return 0;
                    }
                }

            }
            catch (SQLiteException ex)
            {
                await _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex);
                await (transaction?.RollbackAsync() ?? Task.CompletedTask);
                return 0;
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                await (transaction?.RollbackAsync() ?? Task.CompletedTask);
                return 0;
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
