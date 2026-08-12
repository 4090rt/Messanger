using Messangers.Delegate;
using Messangers.ModelData;
using Messangers.SQLite.PoolSQLiteConnection;
using MessangersUI.Delegate;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Data.SQLite;
using System.Transactions;

namespace Messangers.SQLite.ValidationAndRegistrationUserRequest.RequestRegisterAndLogin
{
    public class SaveRequestInBdRegister
    {
        private readonly ILogger<SaveRequestInBdRegister> _logger;
        private readonly PoolSQLite _poolSQLiteConnection;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly ExceptionDelegate _exceptionDelegate;

        public SaveRequestInBdRegister(ILogger<SaveRequestInBdRegister> logger, PoolSQLite poolSQLiteConnection, SQLiteExceptionDelegate sQLiteExceptionDelegate, ExceptionDelegate exceptionDelegate)
        {
            _logger = logger;
            _poolSQLiteConnection = poolSQLiteConnection;
            _sQLiteExceptionDelegate = sQLiteExceptionDelegate;
            _exceptionDelegate = exceptionDelegate;
        }

        public async Task SaveRegisterDataInBd(ModelDataRegister modelDataRegister)
        {
            _logger.LogError("Сохраняю контакт для юзера ");
            SQLiteConnection connection = null;
            SQLiteTransaction sQLiteTransaction = null;
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();

                await using (sQLiteTransaction = connection.BeginTransaction())
                {
                    string comand = $"INSERT INTO [RegisterBase] (TNumber, Mail, Login,Password, DateRegistration) VALUES (@T, @M, @L, @P, @D)";

                    await using (var sqlcommand = new SQLiteCommand(comand, connection, sQLiteTransaction))
                    {
                        sqlcommand.Parameters.AddWithValue("@T", modelDataRegister.Tnumber);
                        sqlcommand.Parameters.AddWithValue("@M", modelDataRegister.Mail);
                        sqlcommand.Parameters.AddWithValue("@L", modelDataRegister.Login);
                        sqlcommand.Parameters.AddWithValue("@P", modelDataRegister.Password);
                        sqlcommand.Parameters.AddWithValue("@D", modelDataRegister.datetime);

                        int rows = await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                        if (rows > 0)
                        {
                            _logger.LogInformation($"Информация о пользователе {modelDataRegister.Login} сохранена. Затронуто строк: {rows}");
                        }
                        else
                        {
                            _logger.LogWarning($"Информация о пользователе {modelDataRegister.Login} не сохранена. Затронуто строк: {rows}");       
                        }
                    }
                    await sQLiteTransaction.CommitAsync().ConfigureAwait(false);
                }
            }
            catch (SQLiteException ex)
            {
                await _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex);
                if (sQLiteTransaction != null)
                {
                    try
                    {
                        await (sQLiteTransaction?.RollbackAsync() ?? Task.CompletedTask);
                    }
                    catch (Exception exTranz)
                    {
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                if (sQLiteTransaction != null)
                {
                    try
                    {
                        await (sQLiteTransaction?.RollbackAsync() ?? Task.CompletedTask);
                    }
                    catch (Exception exTranz)
                    {
                        return;
                    }
                }
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
    }
}
