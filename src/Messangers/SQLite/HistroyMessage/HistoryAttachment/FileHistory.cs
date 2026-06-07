using Messangers.Delegate;
using Messangers.ModelData;
using Messangers.SQLite.PoolSQLiteConnection;
using MessangersUI.Delegate;
using System.Data.SQLite;

namespace Messangers.SQLite.HistroyMessage.HistoryAttachment
{
    public class FileHistory
    {
        private readonly ILogger<FileHistory> _logger;
        private readonly PoolSQLite _poolSQLiteConnection;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly ExceptionDelegate _exceptionDelegate;

        public FileHistory(ILogger<FileHistory> logger, PoolSQLite poolSQLiteConnection, SQLiteExceptionDelegate sQLiteExceptionDelegate, ExceptionDelegate exceptionDelegate)
        {
            _logger = logger;
            _poolSQLiteConnection = poolSQLiteConnection;
            _sQLiteExceptionDelegate = sQLiteExceptionDelegate;
            _exceptionDelegate = exceptionDelegate;
        }

        public async Task< List<AttachmentMetadata>> Request(string user1, string user2)
        {
            SQLiteConnection connection = null;
            SQLiteTransaction transaction = null;
            List<AttachmentMetadata> attachmentMetadataList = new List<AttachmentMetadata>();
            List<string> LoginUser1 = new List<string>();
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();
                transaction = connection.BeginTransaction();

                string command =
                    "SELECT a.*, m.HistoryMessage AS LoginUser1" +
                    " FROM Attachments a JOIN HistoryMessage m ON a.MessageId = m.Id " +
                    "WHERE (m.LoginUser1 = @CurrentUserId AND m.LoginUser2 = @OtherUserId)" +
                    "  OR (m.LoginUser1 = @OtherUserId AND m.LoginUser2 = @CurrentUserId) " +
                    "ORDER BY m.Timestamp DESC";

                await using (var sqlcommand = new SQLiteCommand(command, connection, transaction))
                {
                    sqlcommand.Parameters.AddWithValue("@CurrentUserId", user1);
                    sqlcommand.Parameters.AddWithValue("@OtherUserId", user2);

                    var result = await sqlcommand.ExecuteReaderAsync().ConfigureAwait(false);

                        var attachmentId1 = result.GetOrdinal("Id");
                        var attachmentIdMessage2 = result.GetOrdinal("MessageId");
                        var attachmentFileName3 = result.GetOrdinal("FileName");
                        var attachmentFilePath4 = result.GetOrdinal("FilePath");
                        var attachmentFileSize5 = result.GetOrdinal("FileSize");
                        var attachmentMimeType6 = result.GetOrdinal("MimeType");
                        var attachmentCreateAt7 = result.GetOrdinal("CreatedAt");
                        var attachmentUser8 = result.GetOrdinal("LoginUser1");

                        while (await result.ReadAsync().ConfigureAwait(false))
                        {
                            attachmentMetadataList.Add(new AttachmentMetadata
                            {
                                Id = result.IsDBNull(attachmentId1) ? 0 : result.GetInt32(attachmentId1),
                                MessageId = result.IsDBNull(attachmentIdMessage2) ? 0 : result.GetInt32(attachmentIdMessage2),
                                FileName = result.IsDBNull(attachmentFileName3) ? "" : result.GetString(attachmentFileName3),
                                FilePath = result.IsDBNull(attachmentFilePath4) ? "" : result.GetString(attachmentFilePath4),
                                FileSize = result.IsDBNull(attachmentFileSize5) ? 0 : result.GetInt32(attachmentFileSize5),
                                MimeType = result.IsDBNull(attachmentMimeType6) ? "" : result.GetString(attachmentMimeType6),
                                CreatedAt = result.IsDBNull(attachmentCreateAt7) ? "" : result.GetString(attachmentCreateAt7),
                                User = result.IsDBNull(attachmentUser8) ? "" : result.GetString(attachmentUser8)
                            });
                        }
                        await transaction.CommitAsync().ConfigureAwait(false);
                        return (attachmentMetadataList);
                }

            }
            catch (SQLiteException ex)
            {
                await _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex);
                await (transaction?.RollbackAsync() ?? Task.CompletedTask);
                return (attachmentMetadataList);
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                await (transaction?.RollbackAsync() ?? Task.CompletedTask);
                return (attachmentMetadataList);
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
