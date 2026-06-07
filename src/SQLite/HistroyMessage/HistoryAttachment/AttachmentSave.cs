using Messangers.Delegate;
using Messangers.ModelData;
using Messangers.SQLite.PoolSQLiteConnection;
using MessangersUI.Delegate;
using System.Data.SQLite;

namespace Messangers.SQLite.HistroyMessage.HistoryAttachment
{
    public class AttachmentSave
    {
        private readonly ILogger<AttachmentSave> _logger;
        private readonly PoolSQLite _poolSQLiteConnection;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly ExceptionDelegate _exceptionDelegate;

        public AttachmentSave(ILogger<AttachmentSave> logger, PoolSQLite poolSQLiteConnection, SQLiteExceptionDelegate sQLiteExceptionDelegate, ExceptionDelegate exceptionDelegate)
        {
            _logger = logger;
            _poolSQLiteConnection = poolSQLiteConnection;
            _sQLiteExceptionDelegate = sQLiteExceptionDelegate;
            _exceptionDelegate = exceptionDelegate;
        }

        public async Task<Int64> SaveRequest(AttachmentMetadata attachmentMetadata)
        {
            SQLiteConnection connection = null;
            SQLiteTransaction transaction = null;
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();
                transaction = connection.BeginTransaction();

                string command = "INSERT INTO [Attachments] (FileName, FilePath, FileSize,MimeType) VALUES (@FN, @FP, @FS, @MT); SELECT last_insert_rowid();";

                await using (var commandsql = new SQLiteCommand(command, connection, transaction))
                {
                    commandsql.Parameters.AddWithValue("@FN", attachmentMetadata.FileName);
                    commandsql.Parameters.AddWithValue("@FP", attachmentMetadata.FilePath);
                    commandsql.Parameters.AddWithValue("@FS", attachmentMetadata.FileSize);
                    commandsql.Parameters.AddWithValue("@MT", attachmentMetadata.MimeType);

                    Int64 insertedId = (Int64)await commandsql.ExecuteScalarAsync().ConfigureAwait(false);
                    _logger.LogError($"{insertedId}");
                    if (insertedId > 0)
                    {
                        await transaction.CommitAsync().ConfigureAwait(false);
                        return insertedId;
                    }
                    else
                    {
                        await transaction.RollbackAsync().ConfigureAwait(false);
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
