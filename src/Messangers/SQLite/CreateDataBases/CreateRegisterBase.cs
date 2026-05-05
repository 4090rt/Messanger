using Messangers.Delegate;
using Messangers.ModelData;
using Messangers.SQLite.PoolSQLiteConnection;
using MessangersUI.Delegate;
using System.Data.SQLite;

namespace Messangers.SQLite.CreateDataBases
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
            _logger.LogWarning($"pROVERKA вызван. _isCheckedCreate = {_isCheckedCreate}");  
            if (_isCheckedCreate == true) return;

            if (_isCheckedCreate == false)
            {
                var result1 = await CreateRegisterBases();;

                if (result1)
                {
                    var result2 = await CreateDataBaseUserPRovider();
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
    }
}
