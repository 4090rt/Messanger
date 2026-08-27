using Messangers.DataModel;
using Messangers.Delegate;
using Messangers.SQLite.PoolSQLiteConnection;
using MessangersUI.Delegate;
using System.Data.SQLite;

namespace Messangers.SQLite.Updates.UpdatesPassword
{
    public class UpdatePassword
    {
        private readonly ILogger<UpdatePassword> _logger;
        private readonly PoolSQLite _poolSQLiteConnection;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly ExceptionDelegate _exceptionDelegate;

        private bool _ischekIndex = false;

        public UpdatePassword(ILogger<UpdatePassword> logger, PoolSQLite poolSQLiteConnection, SQLiteExceptionDelegate sQLiteExceptionDelegate, ExceptionDelegate exceptionDelegate)
        {
            _logger = logger;
            _poolSQLiteConnection = poolSQLiteConnection;
            _sQLiteExceptionDelegate = sQLiteExceptionDelegate;
            _exceptionDelegate = exceptionDelegate;

            Task.Run(async () => await InithializateIndex());
        }

        public async Task InithializateIndex()
        {
            if (_ischekIndex == true) return;

            if (_ischekIndex == false)
            {
                await CreateIndex();
                bool result = await IndexProverka();

                _ischekIndex = result;
            }
        }

        public async Task<bool> UpdateRequest(PasswordUpdateStruct data)
        {
            if (string.IsNullOrEmpty(data.UserName) || string.IsNullOrEmpty(data.Password))
                return false;

            SQLiteConnection connection = null;
            SQLiteTransaction sQLiteTransaction = null;  
            try
            { 
                connection = _poolSQLiteConnection.ConnectionOpen();
                sQLiteTransaction= connection.BeginTransaction();

                string command = "UPDATE RegisterBase SET Password = @P WHERE Login = @U";

                await using (SQLiteCommand commandsqlite = new SQLiteCommand(command, connection, sQLiteTransaction))
                {
                    commandsqlite.Parameters.AddWithValue("@P", data.Password);
                    commandsqlite.Parameters.AddWithValue("@U", data.UserName);

                    var result = await commandsqlite.ExecuteNonQueryAsync().ConfigureAwait(false);

                    bool exec = Convert.ToInt32(result) == 1;

                    return exec;
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
                if (sQLiteTransaction != null)
                {
                    sQLiteTransaction.Dispose();
                }
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

                string command = "CREATE INDEX IF NOT EXISTS IX_PasswordUpdate_RegisterBase ON RegisterBase(Login)";

                await using (SQLiteCommand commandsqlite = new SQLiteCommand(command, connection))
                {
                    await commandsqlite.ExecuteNonQueryAsync().ConfigureAwait(false);   
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

                string command = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'index' AND name = 'IX_PasswordUpdate_RegisterBase' AND tbk_name = 'RegisterBase'";

                await using (SQLiteCommand commandsqlite = new SQLiteCommand(command, connection))
                {
                    var result = await commandsqlite.ExecuteNonQueryAsync().ConfigureAwait(false);

                    bool exec = Convert.ToInt32(result) == 1;

                    return exec;
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
