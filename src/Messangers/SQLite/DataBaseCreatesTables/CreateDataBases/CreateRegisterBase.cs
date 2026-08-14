using Messangers.Delegate;
using Messangers.ModelData;
using Messangers.SQLite.PoolSQLiteConnection;
using MessangersUI.Delegate;
using System.Data.SQLite;

namespace Messangers.SQLite.DataBaseCreatesTables.CreateDataBases
{
    public class CreateRegisterBase
    {
        private bool? _isCheckedCreate = false;
        private readonly ILogger<CreateRegisterBase> _logger;
        private readonly PoolSQLite _poolSQLiteConnection;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly ExceptionDelegate _exceptionDelegate;

        public CreateRegisterBase(ILogger<CreateRegisterBase> logger, PoolSQLite poolSQLiteConnection, SQLiteExceptionDelegate sQLiteExceptionDelegate, ExceptionDelegate exceptionDelegate)
        {
            _logger = logger;
            _poolSQLiteConnection = poolSQLiteConnection;
            _sQLiteExceptionDelegate = sQLiteExceptionDelegate;
            _exceptionDelegate = exceptionDelegate;
        }

        public async Task pROVERKA()
        {
            _logger.LogWarning($"pROVERKA !!!!!!!!!!!!!!!!вызван. _isCheckedCreate = {_isCheckedCreate}");  
            if (_isCheckedCreate == true) return;

            if (_isCheckedCreate == false)
            {
                var result1 = await CreateRegisterBases();

                if (result1)
                {
                    var result2 = await CreateDataBaseUserPRovider();

                    if (result2)
                    {
                        var result3 = await CreateDataBaseUserCoNTACT();
                        if (result3)
                        {
                            var result4 = await CreateDataBaseHistroyMesage();
                            if (result4)
                            {
                                var result5 = await CreateFileDataBase();
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogError("CreateRegisterBases вернул false, второй метод не вызван");
                }
            }

            _isCheckedCreate = true;
        }

        public async Task<bool> CreateRegisterBases()
        {
            SQLiteConnection connection = null;
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();

                string command = "CREATE TABLE IF NOT EXISTS RegisterBase (" +
                    "Id INTEGER PRIMARY KEY AUTOINCREMENT," +
                    "TNumber TEXT," +
                    "Mail TEXT," +
                    "Avatar BLOB," +
                    "AvatarExpansion TEXT," +
                    "Login TEXT NOT NULL," +
                    "Password TEXT NOT NULL," +
                    "DateRegistration TEXT NOT NULL)";
                await using (var sqlicommand = new SQLiteCommand(command, connection))
                { 
                     await sqlicommand.ExecuteNonQueryAsync().ConfigureAwait(false);

                     _logger.LogInformation($"База RegisterBase загружена!");
                     return true;
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

        public async Task<bool> CreateDataBaseUserPRovider()
        {
            SQLiteConnection connection = null;
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();

                string command = "CREATE TABLE IF NOT EXISTS ProviderUserBase(" +
                    "Id INTEGER PRIMARY KEY AUTOINCREMENT," +
                    "Location TEXT NOT NULL," +
                    "City TEXT NOT NULL," +
                    "HistName TEXT NOT NULL)";

                await using (var sqlitecomand = new SQLiteCommand(command, connection))
                {
                    await sqlitecomand.ExecuteNonQueryAsync().ConfigureAwait(false);
                    _logger.LogInformation($"База ProviderUserBase загружена!");
                    return true;
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

        public async Task<bool> CreateDataBaseUserCoNTACT()
        {
            SQLiteConnection connection = null;
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();

                string command = "CREATE TABLE IF NOT EXISTS ContactUserBD(" +
                    "Id INTEGER PRIMARY KEY AUTOINCREMENT," +
                    "UserName TEXT NOT NULL," +
                    "LoginContact TEXT NOT NULL," +
                    "Photo TEXT NOT NULL)";

                await using (var commandsql = new SQLiteCommand(command, connection))
                { 
                    await commandsql.ExecuteNonQueryAsync().ConfigureAwait(false);
                    _logger.LogInformation($"База ContactUserBD загружена!");
                    return true;
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

        public async Task<bool> CreateDataBaseHistroyMesage()
        {
            SQLiteConnection connection = null;
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();

                string command = "CREATE TABLE IF NOT EXISTS HistoryMessage(Id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                    "LoginUser1 TEXT NOT NULL, " +
                    "LoginUser2 TEXT NOT NULL," +
                    "Message TEXT NOT NULL," +
                    "Date TEXT NOT NULL," +
                    "State TEXT NOT NULL)";

                await using (var sqlcommand = new SQLiteCommand(command, connection))
                {
                    await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                    _logger.LogInformation($"База HistoryMessage загружена!");
                    return true;
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

        public async Task<bool> CreateFileDataBase()
        {
            SQLiteConnection connection = null;
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();

                string command = "CREATE TABLE IF NOT EXISTS Attachments (" +
               "Id INTEGER PRIMARY KEY AUTOINCREMENT," +
               "MessageId INTEGER," +
               "FileName TEXT NOT NULL," +
               "FilePath TEXT NOT NULL," +
               "FileSize INTEGER NOT NULL," +
               "MimeType TEXT NOT NULL," +
               "CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP," +
               "FOREIGN KEY (MessageId) REFERENCES HistoryMessage(Id) ON DELETE CASCADE" +
               ")";

                await using (var sqlcommand = new SQLiteCommand(command, connection))
                {
                    await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                    _logger.LogInformation($"База FileDataBase загружена!");
                    return true;
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
