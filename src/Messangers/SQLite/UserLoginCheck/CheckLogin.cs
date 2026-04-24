using Messangers.Delegate;
using Messangers.SQLite.PoolSQLiteConnection;
using Messangers.SQLite.RequestRegisterAndLogin;
using MessangersUI.Delegate;
using System.Data.SQLite;

namespace Messangers.SQLite.UserLoginCheck
{
    public class CheckLogin
    {
        private readonly ILogger<CheckLogin> _logger;
        private readonly PoolSQLite _poolSQLiteConnection;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly ExceptionDelegate _exceptionDelegate;
        private bool _Is_chekedindex = false;

        public CheckLogin(ILogger<CheckLogin> logger, PoolSQLite poolSQLiteConnection, SQLiteExceptionDelegate sQLiteExceptionDelegate, ExceptionDelegate exceptionDelegate)
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
                await IndexCreate();
                await IndexCheck();
            }

            _Is_chekedindex = true;
        }

        public async Task<bool> RequestForLogin(string Login, CancellationToken cancellation = default)
        {
            SQLiteConnection connection = null;
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();

                string command = "SELECT Login FROM RegisterBase WHERE Login = @L";

                await using (var sqlcommand = new SQLiteCommand(command, connection))
                {
                    sqlcommand.Parameters.AddWithValue("@L", Login);

                    var result = await sqlcommand.ExecuteScalarAsync().ConfigureAwait(false);

                    if (result != null)
                    {
                        bool exec = Convert.ToInt32(result) == 1;

                        if (exec)
                        {
                            _logger.LogWarning($"Пользователь {Login} уже зарегестрирован!");
                            return false;
                        }
                        return false;
                    }
                    else
                    {
                        _logger.LogWarning($"Не удалось проверить наличие юзера <Login> в базе, Можно регестрировать");
                        return true;
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

        public async Task IndexCreate()
        { 
            SQLiteConnection connection = null;
            try
            { 
                connection = _poolSQLiteConnection.ConnectionOpen();

                string command = $"CREATE INDEX IF NOT EXISTS IX_RegisterBase_LoginCheck ON RegisterBase(Login)";

                await using (var sqlcommand = new SQLiteCommand(command, connection))
                {
                    await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                    _logger.LogWarning("Индекс X_RegisterBase_LoginCheck создан ");
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

                string command = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'index' " +
                    "AND name = 'IX_RegisterBase_LoginCheck' AND tbl_name = 'RegisterBase'";

                await using (var sqlcommand = new SQLiteCommand(command, connection))
                {
                    var result = await sqlcommand.ExecuteScalarAsync().ConfigureAwait(false);
                    if (result != null)
                    {
                        bool exec = Convert.ToInt32(result) == 1;

                        if (exec)
                        {
                            _logger.LogWarning("Индекс X_RegisterBase_LoginCheck успешно создан и проверен!");
                            return true;
                        }
                        else
                        {
                            _logger.LogWarning("Индекса X_RegisterBase_LoginCheck не существует");
                            return false;
                        }
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
                    _poolSQLiteConnection.CloseConnection(connection);
                }
            }
        }
    }
}
