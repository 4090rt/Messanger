using Messangers.Delegate;
using Messangers.ModelData;
using Messangers.SQLite.PoolSQLiteConnection;
using Messangers.SQLite.ValidationAndRegistrationUserRequest.RequestRegisterAndLogin;
using MessangersUI.DataModel;
using MessangersUI.Delegate;
using System.Data.SQLite;

namespace Messangers.SQLite.ContactBse.UserSave
{
    public class SaveClass
    {
        private readonly ILogger<SaveClass> _logger;
        private readonly PoolSQLite  _poolSQLiteConnection;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly ExceptionDelegate _exceptionDelegate;

        public SaveClass(ILogger<SaveClass> logger, PoolSQLite poolSQLiteConnection, SQLiteExceptionDelegate sQLiteExceptionDelegate, ExceptionDelegate exceptionDelegate)
        {
            _logger = logger;
            _poolSQLiteConnection = poolSQLiteConnection;
            _sQLiteExceptionDelegate = sQLiteExceptionDelegate;
            _exceptionDelegate = exceptionDelegate;
        }

        public async Task SaveMethod(List<UserContact> list)
        {
            SQLiteConnection connection = null;
            SQLiteTransaction transaction = null;
            try
            {
                var model = list.FirstOrDefault();
                if (model != null)
                {
                    connection = _poolSQLiteConnection.ConnectionOpen();
                    transaction = connection.BeginTransaction();

                    string command = "INSERT INTO [ContactUserBD] (UserName, LoginContact, Photo) VALUES (@U, @L, @P)";

                    await using (var sqlcommand = new SQLiteCommand(command, connection, transaction))
                    {
                        sqlcommand.Parameters.AddWithValue("@U", model.Username);
                        sqlcommand.Parameters.AddWithValue("@L", model.Name);
                        sqlcommand.Parameters.AddWithValue("@P", model.photo);

                        int rows = await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                        if (rows > 0)
                        {
                        }
                        else
                        {
                        }
                    }
                    await transaction.CommitAsync().ConfigureAwait(false);
                }
            }
            catch (SQLiteException ex)
            {
                await _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex);
                await (transaction?.RollbackAsync() ?? Task.CompletedTask);
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                await (transaction?.RollbackAsync() ?? Task.CompletedTask);
            }
            finally
            {
                if (connection != null)
                {
                    _poolSQLiteConnection.CloseConnection(connection);
                }
                if (transaction != null)
                {
                    transaction.Dispose();
                }
            }
        }
    }
}
