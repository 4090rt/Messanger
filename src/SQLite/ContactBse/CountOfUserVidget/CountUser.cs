using Messangers.Delegate;
using Messangers.SQLite.PoolSQLiteConnection;
using MessangersUI.Delegate;
using Microsoft.Extensions.Caching.Memory;
using System.Data.SQLite;

namespace Messangers.SQLite.ContactBse.CountOfUserVidget
{
    public class CountUser
    {
        private readonly ILogger<CountUser> _logger;
        private readonly Messangers.SQLite.PoolSQLiteConnection.PoolSQLite _poolSQLite;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly ExceptionDelegate _exceptionDelegate;
        private bool _Is_chekedindex = false;

        public CountUser(ILogger<CountUser> logger, Messangers.SQLite.PoolSQLiteConnection.PoolSQLite poolSQLiteConnection, SQLiteExceptionDelegate sQLiteExceptionDelegate, ExceptionDelegate exceptionDelegate)
        {
            _logger = logger;
            _poolSQLite = poolSQLiteConnection;
            _sQLiteExceptionDelegate = sQLiteExceptionDelegate;
            _exceptionDelegate = exceptionDelegate;

            try
            {
                Task.Run(async () => await Inithializate()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка инициализации CountUser");
            }
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

        public async Task<int> Count(string username)
        { 
            SQLiteConnection connection = null;
            SQLiteTransaction transaction = null;
            int count = 0;
            try
            {
                _logger.LogError(username);
                connection = _poolSQLite.ConnectionOpen();
                transaction = connection.BeginTransaction();

                string command = "SELECT COUNT(*) FROM ContactUserBD WHERE Name = @N";

                await using (var sqlcommand = new SQLiteCommand(command, connection, transaction))
                {
                    sqlcommand.Parameters.AddWithValue("@N", username);

                    var result = await sqlcommand.ExecuteScalarAsync().ConfigureAwait(false);

                    int countr = Convert.ToInt32(result);

                    if (countr > 0)
                    {
                        count = countr;
                    }  
                    await transaction.CommitAsync().ConfigureAwait(false);  
                    return count;
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
                    _poolSQLite.CloseConnection(connection);
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
                connection = _poolSQLite.ConnectionOpen();

                string command = "CREATE INDEX IF NOT EXISTS IX_ContactUserBDCountContacts ON ContactUserBD(Name)";

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
                    _poolSQLite.CloseConnection(connection);
                }
            }
        }

        public async Task<bool> IndexProverka() 
        {
            SQLiteConnection connection = null;
            try
            {
                connection = _poolSQLite.ConnectionOpen();

                string command = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'index' AND name = 'IX_ContactUserBDCountContacts' AND tbl_name = 'ContactUserBD'";

                await using (var sqlcommand = new SQLiteCommand(command, connection))
                { 
                    var result = await sqlcommand.ExecuteScalarAsync().ConfigureAwait(false);

                    if (result != null)
                    {
                        bool exec = Convert.ToInt32(result) == 1;

                        if (exec)
                        {
                            _logger.LogWarning("Индекс IX_ContactUserBDCountContacts создан!");
                            return true;
                        }
                        return false;
                    }
                    return false;
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
                    _poolSQLite.CloseConnection(connection);
                }
            }
        }
    }
}
