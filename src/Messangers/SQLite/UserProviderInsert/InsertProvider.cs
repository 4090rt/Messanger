using Messangers.Delegate;
using Messangers.ModelData;
using Messangers.SQLite.PoolSQLiteConnection;
using Messangers.SQLite.RequestRegisterAndLogin;
using MessangersUI.Delegate;
using System.Data.SQLite;

namespace Messangers.SQLite.UserProviderInsert
{
    public class InsertProvider
    {
        private readonly ILogger<InsertProvider> _logger;
        private readonly PoolSQLite _poolSQLiteConnection;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly ExceptionDelegate _exceptionDelegate;

        public InsertProvider(ILogger<InsertProvider> logger,
            PoolSQLite poolSQLiteConnection,
            SQLiteExceptionDelegate sQLiteExceptionDelegate,
            ExceptionDelegate exceptionDelegate)
        {
            _logger = logger;
            _poolSQLiteConnection = poolSQLiteConnection;
            _sQLiteExceptionDelegate = sQLiteExceptionDelegate;
            _exceptionDelegate = exceptionDelegate;
        }

        public async Task<bool> InsertRequest(IpIfo list)
        {
            SQLiteConnection connection = null;
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();

                string command = "INSERT INTO [ProviderUserBase]" +
                    " (Location, City, HistName)" +
                    " VALUES (@LOC, @C, @H)";

                    await using (var commandsql = new SQLiteCommand(command, connection))
                    {
                        commandsql.Parameters.AddWithValue("@LOC", list.Loc);
                        commandsql.Parameters.AddWithValue("@C", list.City);
                        commandsql.Parameters.AddWithValue("@H", list.Hostname);
                        
                        var result = await commandsql.ExecuteScalarAsync().ConfigureAwait(false);

                        if (result != null)
                        {
                            bool exec = Convert.ToInt32(result) == 1;

                            if (exec)
                            {
                                _logger.LogWarning("Провайдер добавлен");
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
                return true;
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
