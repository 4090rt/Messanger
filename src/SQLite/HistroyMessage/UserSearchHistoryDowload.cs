using Messangers.Delegate;
using Messangers.ModelData;
using Messangers.SQLite.PoolSQLiteConnection;
using MessangersUI.Delegate;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Memory;
using System.Data.SQLite;
using System.Reflection.PortableExecutable;

namespace Messangers.SQLite.HistroyMessage
{
    public class UserSearchHistoryDowload
    {
        private readonly ILogger<UserSearchHistoryDowload> _logger;
        private readonly PoolSQLite _poolSQLiteConnection;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly ExceptionDelegate _exceptionDelegate;
        private bool _Is_chekedindex = false;

        public  UserSearchHistoryDowload(ILogger<UserSearchHistoryDowload> logger, PoolSQLite poolSQLiteConnection,
            SQLiteExceptionDelegate sQLiteExceptionDelegate,
            ExceptionDelegate exceptionDelegate)
        {
            _logger = logger;
            _poolSQLiteConnection = poolSQLiteConnection;
            _sQLiteExceptionDelegate = sQLiteExceptionDelegate;
            _exceptionDelegate = exceptionDelegate;

            Task.Run(async () => await Inithializate()).ConfigureAwait(false);
        }

        public async Task Inithializate()
        {
            if (_Is_chekedindex) return;

            if (_Is_chekedindex == false)
            {
                await CreateIndex();
                await IndexProverka();
            }

            _Is_chekedindex = true;
        }

        public async Task<List<MessageData>> SelectRequest(string LoginUser1, string LoginUser2)
        { 
            SQLiteConnection connection = null;
            List<MessageData> list = new List<MessageData>();
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();

                string command = @"
                SELECT 
                    m.Id AS MessageId,
                    m.LoginUser1,
                    m.LoginUser2,
                    m.Message,
                    m.Date,
                    a.Id AS AttachmentId,
                    a.FileName,
                    a.FilePath,
                    a.FileSize,
                    a.MimeType
                FROM HistoryMessage m
                LEFT JOIN Attachments a ON m.Id = a.MessageId
                WHERE (m.LoginUser1 = @U1 AND m.LoginUser2 = @U2)
                   OR (m.LoginUser1 = @U2 AND m.LoginUser2 = @U1)
                ORDER BY m.Date ASC";

                await using (var sqlcommand = new SQLiteCommand(command, connection))
                {
                    sqlcommand.Parameters.AddWithValue("@U1", LoginUser1);
                    sqlcommand.Parameters.AddWithValue("@U2", LoginUser2);

                    await using  var result = await sqlcommand.ExecuteReaderAsync().ConfigureAwait(false);

                    int idcUser0 = result.GetOrdinal("Id");
                    int idxUser1 = result.GetOrdinal("LoginUser1");
                    int idxUser2 = result.GetOrdinal("LoginUser2");
                    int idxMessage = result.GetOrdinal("Message");
                    int idxDate = result.GetOrdinal("Date");

                    while (await result.ReadAsync().ConfigureAwait(false))
                    {
                        list.Add(new MessageData
                        {
                            Id = result.IsDBNull(idcUser0) ? 0 : result.GetInt32(idcUser0),
                            LoginUser1 = result.IsDBNull(idxUser1) ? "" : result.GetString(idxUser1),
                            LoginUser2 = result.IsDBNull(idxUser2) ? "" : result.GetString(idxUser2),
                            Message = result.IsDBNull(idxMessage) ? "" : result.GetString(idxMessage),
                            Date = result.IsDBNull(idxDate) ? "" : result.GetString(idxDate)
                        });
                    }
                    return list;
                }
            }
            catch (SQLiteException ex)
            {
                await _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex);
                return new List<MessageData>();
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return new List<MessageData>();
            }
            finally
            {
                if (connection != null)
                {
                    _poolSQLiteConnection.CloseConnection(connection);
                }
            }
        }

        public async Task CreateIndex()
        {
            SQLiteConnection connection = null;
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();

                string command = "CREATE INDEX IF NOT EXISTS IX_ContactUserBD_iNDEXHistory ON HistoryMessage(LoginUser1, LoginUser2)";

                await using (var sqlitecommand = new SQLiteCommand(command, connection))
                { 
                    await sqlitecommand.ExecuteNonQueryAsync().ConfigureAwait(false);
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

                string command = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'index' AND name = 'IX_ContactUserBD_iNDEXHistory' AND tbl_name = 'HistoryMessage'";

                await using (var sqlcommand = new SQLiteCommand(command, connection))
                { 
                    var result = await sqlcommand.ExecuteScalarAsync().ConfigureAwait(false);

                    bool exec = Convert.ToInt32(result) == 1;

                    if (exec)
                    {
                        _logger.LogInformation("Индекс IX_ContactUserBD_iNDEXHistory Создан!");
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
