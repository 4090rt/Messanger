using Messangers.Delegate;
using Messangers.SQLite.PoolSQLiteConnection;
using MessangersUI.Delegate;
using System.Data.SQLite;

namespace Messangers.SQLite.HistroyMessage.HistoryAttachment
{
    public class AttachmentIdUpdate
    {
        private readonly ILogger<AttachmentIdUpdate> _logger;
        private readonly PoolSQLite _poolSQLiteConnection;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly ExceptionDelegate _exceptionDelegate;

        public AttachmentIdUpdate(ILogger<AttachmentIdUpdate> logger, PoolSQLite poolSQLiteConnection, SQLiteExceptionDelegate sQLiteExceptionDelegate, ExceptionDelegate exceptionDelegate)
        {
            _logger = logger;
            _poolSQLiteConnection = poolSQLiteConnection;
            _sQLiteExceptionDelegate = sQLiteExceptionDelegate;
            _exceptionDelegate = exceptionDelegate;
        }

        public async Task<bool> RequestUpdate(int id, Int64 attachid)
        {
            SQLiteConnection connection = null;
            SQLiteTransaction transaction = null;
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();
                transaction = connection.BeginTransaction();
                _logger.LogError($"{id}");
                _logger.LogError($"{attachid}");
                string command = "UPDATE Attachments SET MessageId = @Id WHERE Id = @attachId";

                await using (var sqlcommand = new SQLiteCommand(command, connection, transaction))
                {
                    sqlcommand.Parameters.AddWithValue("@attachId", attachid);
                    sqlcommand.Parameters.AddWithValue("@Id", id);

                    int rowsAffected = await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);

                    _logger.LogError($"Затронуто строк: {rowsAffected}");

                    if (rowsAffected > 0)
                    {
                        await transaction.CommitAsync().ConfigureAwait(false);
                        return true;
                    }
                    else
                    {
                        await transaction.RollbackAsync().ConfigureAwait(false);
                        return false;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                await _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex);
                await (transaction?.RollbackAsync() ?? Task.CompletedTask);
                return false;
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                await (transaction?.RollbackAsync() ?? Task.CompletedTask);
                return false;
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
