using Messangers.Delegate;
using Messangers.SQLite.PoolSQLiteConnection;
using MessangersUI.Delegate;
using System.Data.SQLite;

namespace Messangers.SQLite.UserLoginCheck
{
    public class CheckHashPasswordFromBD
    {
        private readonly PoolSQLite _poolSQLiteConnection;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly ExceptionDelegate _exceptionDelegate;
        private bool _Is_chekedindex = false;
        private readonly ILogger<CheckHashPasswordFromBD> _logger;

        public CheckHashPasswordFromBD(ILogger<CheckHashPasswordFromBD> logger, PoolSQLite poolSQLiteConnection, SQLiteExceptionDelegate sQLiteExceptionDelegate, ExceptionDelegate exceptionDelegate)
        {
            _logger = logger;
            _poolSQLiteConnection = poolSQLiteConnection;
            _sQLiteExceptionDelegate = sQLiteExceptionDelegate;
            _exceptionDelegate = exceptionDelegate;

            Task.Run(async () => await Inithializate()).ConfigureAwait(false);
        }

        public async Task Inithializate()
        {
            if (_Is_chekedindex == true) return;

            if (_Is_chekedindex == false)
            {
                await CreateIdex();
                await IndexCheck();
            }

           _Is_chekedindex = true;
        }

        public async Task<bool> PasswordCheckRequest(string Login, string Password)
        {
            SQLiteConnection connection = null;
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();

                string command = "SELECT Password FROM RegisterBase WHERE Login = @Login AND [Password] = @Password";

                await using (var sqlcommand = new SQLiteCommand(command, connection))
                {
                    sqlcommand.Parameters.AddWithValue("@Password", Password);
                    sqlcommand.Parameters.AddWithValue("@Login", Login);

                    var result = await sqlcommand.ExecuteScalarAsync().ConfigureAwait(false);

                    if (result != null && Password == result.ToString())
                    {
                        return true;
                    }
                    else
                    {
                        _logger.LogError($"не удалось проверить пароля для {Login}");
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

        public async Task CreateIdex()
        {
            SQLiteConnection connection = null;

            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();

                string command = "CREATE INDEX IF NOT EXISTS IX_RegisterBase_Password ON RegisterBase(Password)";

                await using (var SQLCOMMAND = new SQLiteCommand(command, connection))
                { 
                    await SQLCOMMAND.ExecuteNonQueryAsync().ConfigureAwait(false);
                    _logger.LogWarning("индекс IX_RegisterBase_Password создан!");
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

        public async Task<bool> IndexCheck()
        {
            SQLiteConnection connection = null;
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();

                string coomand = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'index' AND NAME = 'IX_RegisterBase_Password' AND tbl_name = 'RegisterBase'";

                await using (var sqlcommand = new SQLiteCommand(coomand, connection))
                { 
                    var result = await sqlcommand.ExecuteScalarAsync().ConfigureAwait(false);

                    if (result != null)
                    {
                        bool exec = Convert.ToInt32(result) == 1;

                        if (exec)
                        {
                            _logger.LogWarning("Индекс IX_RegisterBase_Password проверен");
                            return true;
                        }
                        return false;
                    }
                    else
                    {
                        _logger.LogWarning($"Не удалось проверить индекс IX_RegisterBase_Password");
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
