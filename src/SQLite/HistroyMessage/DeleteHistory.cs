using Messangers.Delegate;
using Messangers.SQLite.PoolSQLiteConnection;
using MessangersUI.Delegate;
using System.Data.SQLite;
using System.Threading.Tasks.Dataflow;

namespace Messangers.SQLite.HistroyMessage
{
    public class DeleteHistory
    {
        private readonly ILogger<DeleteHistory> _logger;
        private readonly PoolSQLite _poolSQLiteConnection;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly ExceptionDelegate _exceptionDelegate;
        public bool _ischekedindex = false;

        public DeleteHistory(ILogger<DeleteHistory> logger, PoolSQLite poolSQLiteConnection, SQLiteExceptionDelegate sQLiteExceptionDelegate, ExceptionDelegate exceptionDelegate)
        {
            _logger = logger;
            _poolSQLiteConnection = poolSQLiteConnection;
            _sQLiteExceptionDelegate = sQLiteExceptionDelegate;
            _exceptionDelegate = exceptionDelegate;

            Task.Run(async () => await Inithializate());
        }

        public async Task Inithializate()
        {
            if (_ischekedindex) return;

            if (_ischekedindex == false)
            { 
                await CreateIndex();
                await IndexProverka();
            }

            _ischekedindex = true;
        }

        public async Task RequestDelete(string user, string username)
        { 
            SQLiteConnection connection = null;
            SQLiteTransaction transaction = null;
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();
                transaction = connection.BeginTransaction();
                string command = @"DELETE FROM HistoryMessage 
               WHERE (LoginUser1 = @U1 AND LoginUser2 = @U2)
                  OR (LoginUser1 = @U2 AND LoginUser2 = @U1)";

                await using (var sqlcommand = new SQLiteCommand(command, connection, transaction))
                {
                    sqlcommand.Parameters.AddWithValue("@U1", user);
                    sqlcommand.Parameters.AddWithValue("@U2", username);

                    await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
                await transaction.CommitAsync().ConfigureAwait(false);
            }
            catch (SQLiteException ex)
            {
                _logger.LogError(ex.Message);
                await _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex);
                await (transaction?.RollbackAsync() ?? Task.CompletedTask);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                await (transaction.RollbackAsync() ?? Task.CompletedTask);
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

        public async Task CreateIndex()
        {
            SQLiteConnection connection = null;
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();

                string command = "CREATE INDEX IF NOT EXISTS IX_HistroyMessageDelete ON HistoryMessage(LoginUser1, LoginUser2)";

                await using (var sqlcommand = new SQLiteCommand(command, connection))
                {
                    _logger.LogWarning("Индекс IX_HistroyMessageDelete Создан");
                    await sqlcommand.ExecuteNonQueryAsync();
                }
            }
            catch (SQLiteException ex)
            {
                await _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
            }
            finally
            {
                if (connection != null)
                {
                    _poolSQLiteConnection.CloseConnection(connection);
                }
            }
        }

        public async Task<bool> IndexProverka()
        {
            SQLiteConnection connection = null;
            try
            {
                connection= _poolSQLiteConnection.ConnectionOpen();

                string command = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'index' AND name = 'IX_HistroyMessageDelete' and tbl_name = 'HistoryMessage'";

                await using (var sqlcommand = new SQLiteCommand(command, connection))
                {
                    var result = await sqlcommand.ExecuteScalarAsync().ConfigureAwait(false);

                    bool exec = Convert.ToInt32(result) == 1;

                    if (exec)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                await _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return false;
            }
            finally
            {
                if (connection != null)
                {
                    _poolSQLiteConnection.CloseConnection(connection);
                }
            }
        }
    }
}
