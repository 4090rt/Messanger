using Messangers.Delegate;
using Messangers.SQLite.ContactBse.UserSearchContact;
using Messangers.SQLite.DataBaseCreatesTables.PoolSQLiteConnection;
using MessangersUI.Delegate;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Memory;
using System.Data.SQLite;

namespace Messangers.SQLite.ContactBse.DeleteContact
{
    public class DeleteContact
    {
        private readonly ILogger<DeleteContact> _logger;
        private readonly PoolSQLite _poolSQLiteConnection;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly IMemoryCache _memorycache;
        private bool _Is_chekedindex = false;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public DeleteContact(ILogger<DeleteContact> logger, PoolSQLite poolSQLiteConnection,
            SQLiteExceptionDelegate sQLiteExceptionDelegate,
            ExceptionDelegate exceptionDelegate, IMemoryCache memoryCache)
        {
            _logger = logger;
            _poolSQLiteConnection = poolSQLiteConnection;
            _sQLiteExceptionDelegate = sQLiteExceptionDelegate;
            _exceptionDelegate = exceptionDelegate;
            _memorycache = memoryCache;

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

        public async Task Request(string username, string logincontact)
        {
            SQLiteConnection connection = null;
            SQLiteTransaction sqliteTransaction = null;
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();
                sqliteTransaction = connection.BeginTransaction();

                string command = "DELETE FROM ContactUserBD WHERE Name = @N AND UserName = @U";  
                await using (var sqlcommand = new SQLiteCommand(command, connection, sqliteTransaction))
                {
                    sqlcommand.Parameters.AddWithValue("@N", username);
                    sqlcommand.Parameters.AddWithValue("@U", logincontact);

                    await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
                await sqliteTransaction.CommitAsync().ConfigureAwait(false);
            }
            catch (SQLiteException ex)
            {
                await _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex);
                await (sqliteTransaction?.RollbackAsync() ?? Task.CompletedTask);
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                await (sqliteTransaction?.RollbackAsync() ?? Task.CompletedTask);
            }
            finally
            {
                if (connection != null)
                {
                    _poolSQLiteConnection.CloseConnection(connection);
                }
                if (sqliteTransaction != null)
                {
                    sqliteTransaction.Dispose();
                }
            }
        }

        public async Task CreateIndex()
        {
            SQLiteConnection connection = null;
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();

                string command = "CREATE INDEX IF NOT EXISTS IX_ContactUserBD_DELETEINDEX ON ContactUserBD(Name, UserName)";

                await using (var sqlcommand = new SQLiteCommand(command, connection))
                {
                    await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);
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

                string command = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'index' AND name = 'IX_ContactUserBD_DELETEINDEX ' AND tbl_name = 'ContactUserBD'";

                await using (var sqlcommand = new SQLiteCommand(command, connection))
                { 
                    var result = await sqlcommand.ExecuteScalarAsync().ConfigureAwait(false);

                    if (result != null)
                    {
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
