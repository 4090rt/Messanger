using Messangers.Delegate;
using Messangers.SQLite.PoolSQLiteConnection;
using MessangersUI.Delegate;
using Microsoft.Data.Sqlite;
using System.Data.SQLite;
using System.Transactions;

namespace Messangers.SQLite.HistroyMessage
{
    public class DeleteConcrectMessage
    {
        private readonly ILogger<DeleteConcrectMessage> _logger;
        private readonly PoolSQLite _poolSQLiteConnection;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly ExceptionDelegate _exceptionDelegate;
        public bool _ischekedindex = false;

        public DeleteConcrectMessage(ILogger<DeleteConcrectMessage> logger, PoolSQLite poolSQLiteConnection, SQLiteExceptionDelegate sQLiteExceptionDelegate, ExceptionDelegate exceptionDelegate)
        {
            _logger = logger;
            _poolSQLiteConnection = poolSQLiteConnection;
            _sQLiteExceptionDelegate = sQLiteExceptionDelegate;
            _exceptionDelegate = exceptionDelegate;

            Task.Run(async () => await Inithializate());
        }

        public async Task Inithializate()
        {

        }

        public async Task RequestDelete(int id)
        {
            SQLiteConnection connection = null;
            SQLiteTransaction transaction = null;
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();
                transaction = connection.BeginTransaction();

                string command = "DELETE FROM HistoryMessage WHERE (Id = @Id)";

                await using (var sqlcommand = new SQLiteCommand(command, connection, transaction))
                {
                    sqlcommand.Parameters.AddWithValue("@Id", id);
                    
                    await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);
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

        public async Task CreateIndex()
        {
            SQLiteConnection connection = null;

            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();

                string command = "CREATE INDEX IF NOT EXISTS IX_HistoryMessageDeleteConcret ON HistoryMessage(Id)";

                await using (var sqlcommand = new SQLiteCommand(command, connection))
                {
                    await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                    _logger.LogWarning("Индекс IX_HistoryMessageDeleteConcret Создан");
                }
            }
            catch (SQLiteException ex)
            {
                await _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex);

            }
            catch (Exception ex)
            {
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
                connection = _poolSQLiteConnection.ConnectionOpen();

                string command = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'index' AND name = 'IX_HistoryMessageDeleteConcret' AND tbl_name = 'HistoryMessage'";

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
