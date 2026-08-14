using Messangers.Delegate;
using Messangers.SQLite.DataBaseCreatesTables.CreateDataBases;
using Messangers.SQLite.PoolSQLiteConnection;
using MessangersUI.DataModel;
using MessangersUI.Delegate;
using System.Data.SQLite;

namespace Messangers.SQLite.AvatarAdd
{
    public class AvatarUpdate
    {
        private bool? _isCheckedCreate = false;
        private readonly ILogger<CreateRegisterBase> _logger;
        private readonly PoolSQLite _poolSQLiteConnection;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly InvalidExcaptionDelegate _invalidExcaptionDelegate;

        public AvatarUpdate(ILogger<CreateRegisterBase> logger, PoolSQLite poolSQLiteConnection,
            SQLiteExceptionDelegate sQLiteExceptionDelegate, ExceptionDelegate exceptionDelegate, InvalidExcaptionDelegate invalidExcaptionDelegate)
        {
            _logger = logger;
            _poolSQLiteConnection = poolSQLiteConnection;
            _sQLiteExceptionDelegate = sQLiteExceptionDelegate;
            _exceptionDelegate = exceptionDelegate;
            _invalidExcaptionDelegate = invalidExcaptionDelegate;

            Task.Run(async () => await IndexCheked());
        }

        public async Task IndexCheked()
        {
            if (_isCheckedCreate == true) return;

            if (_isCheckedCreate == false)
            {
                await CreateIndex();
                bool result = await IndexProverka();

                if (result == true)
                    _isCheckedCreate = true;
                else
                    _isCheckedCreate = false;
            }
        }

        public async Task RequestUpdAvatar(AvatarMetaData avatarMetaData)
        {
            SQLiteConnection connection = null;
            SQLiteTransaction transaction = null;
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();
                transaction = connection.BeginTransaction();

                string command = "UPDATE RegisterBase SET Avatar = @A,AvatarExpansion = @AE WHERE Login = @U";

                await using (var sqlcommand = new SQLiteCommand(command, connection, transaction))
                {
                    sqlcommand.Parameters.AddWithValue("@A", avatarMetaData.File);
                    sqlcommand.Parameters.AddWithValue("@AE", avatarMetaData.expansion);
                    sqlcommand.Parameters.AddWithValue("@U", avatarMetaData.UserName);

                    await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);

                    await transaction.CommitAsync().ConfigureAwait(false);
                }
            }
            catch (SQLiteException ex)
            {
                await _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex).ConfigureAwait(false);
                try
                {
                    if (transaction != null)
                    {
                        await (transaction?.RollbackAsync() ?? Task.CompletedTask);
                    }
                    else
                    {
                        return;
                    }
                }
                catch (Exception exTransaction)
                {
                    await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, exTransaction).ConfigureAwait(false);
                }
            }
            catch (InvalidOperationException ex)
            {
                await _invalidExcaptionDelegate.RunDelegate(ex).ConfigureAwait(false);
                try
                {
                    if (transaction != null)
                    {
                        await (transaction?.RollbackAsync() ?? Task.CompletedTask);
                    }
                    else
                    {
                        return;
                    }
                }
                catch (Exception exTransaction)
                {
                    await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, exTransaction).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex).ConfigureAwait(false);
                try
                {
                    if (transaction != null)
                    {
                        await (transaction?.RollbackAsync() ?? Task.CompletedTask);
                    }
                    else
                    {
                        return;
                    }
                }
                catch (Exception exTransaction)
                {
                    await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, exTransaction).ConfigureAwait(false);
                }
            }
            finally
            {
                if (connection != null)
                {
                    _poolSQLiteConnection.CloseConnection(connection);

                    if (transaction != null)
                        transaction.Dispose();
                }
            }
        }

        public async Task CreateIndex()
        {
            SQLiteConnection connection = null;
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();

                string command = "CREATE INDEX IF NOT EXISTS IX_IndexAvatarSearchLogin ON  RegisterBase(Login)";

                await using (var commandsql = new SQLiteCommand(command, connection))
                { 
                    await commandsql.ExecuteNonQueryAsync().ConfigureAwait(false);
                    _logger.LogWarning("Индекс IX_IndexAvatarSearchLogin Создан");
                }
            }
            catch (SQLiteException ex)
            {
                await _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex).ConfigureAwait(false);
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

                string command = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'index' AND name = 'IX_IndexAvatarSearchLogin' and tbl_name = 'RegisterBase'";

                await using (var sqlcommand = new SQLiteCommand(command, connection))
                { 
                    var result = await sqlcommand.ExecuteScalarAsync().ConfigureAwait(false);

                    bool exec = Convert.ToInt32(result) == 1;

                    return exec;
                }
            }
            catch (SQLiteException ex)
            {
                await _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex).ConfigureAwait(false);
                return false;
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex).ConfigureAwait(false);
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
