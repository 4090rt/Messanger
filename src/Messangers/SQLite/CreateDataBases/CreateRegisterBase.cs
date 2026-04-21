using Messangers.Delegate;
using Messangers.ModelData;
using Messangers.SQLite.PoolSQLiteConnection;
using MessangersUI.Delegate;
using System.Data.SQLite;

namespace Messangers.SQLite.CreateDataBases
{
    public class CreateRegisterBase
    {
        private bool? _isCheckedCreate;
        private readonly ILogger<CreateRegisterBase> _logger;
        private readonly PoolSQLite _poolSQLiteConnection;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly ExceptionDelegate _exceptionDelegate;

        public CreateRegisterBase(bool? isCheckedCreate, ILogger<CreateRegisterBase> logger, PoolSQLite poolSQLiteConnection, SQLiteExceptionDelegate sQLiteExceptionDelegate, ExceptionDelegate exceptionDelegate)
        {
            _isCheckedCreate = isCheckedCreate;
            _logger = logger;
            _poolSQLiteConnection = poolSQLiteConnection;
            _sQLiteExceptionDelegate = sQLiteExceptionDelegate;
            _exceptionDelegate = exceptionDelegate;
        }

        public async Task pROVERKA()
        {
            if (_isCheckedCreate == true) return;

            if (_isCheckedCreate == false)
            { 
                
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
                    "DateRegistrftion, TEXT NOT NULL)";
                await using (var sqlicommand = new SQLiteCommand(command, connection))
                { 
                    var result = await sqlicommand.ExecuteScalarAsync().ConfigureAwait(false);

                    bool exec = Convert.ToInt32(result) == 1;

                    if (exec)
                    {
                        _logger.LogInformation($"База RegisterBase загружена!");
                        return true;
                    }
                    else
                    {
                        _logger.LogInformation($"Ошибка заргузки базы RegisterBase");
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
