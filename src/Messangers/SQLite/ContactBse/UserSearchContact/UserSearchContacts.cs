using Messangers.Delegate;
using Messangers.SQLite.ContactBse.UserSerach;
using Messangers.SQLite.PoolSQLiteConnection;
using MessangersUI.DataModel;
using MessangersUI.Delegate;
using Microsoft.Extensions.Caching.Memory;
using System.Data.SQLite;

namespace Messangers.SQLite.ContactBse.UserSearchContact
{
    public class UserSearchContacts
    {
        private readonly ILogger<UserSearchContacts> _logger;
        private readonly PoolSQLite _poolSQLiteConnection;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly IMemoryCache _memorycache;
        private bool _Is_chekedindex = false;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public UserSearchContacts(ILogger<UserSearchContacts> logger, PoolSQLite poolSQLiteConnection,
            SQLiteExceptionDelegate sQLiteExceptionDelegate,
            ExceptionDelegate exceptionDelegate, IMemoryCache memoryCache)
        {
            _logger = logger;
            _poolSQLiteConnection = poolSQLiteConnection;
            _sQLiteExceptionDelegate = sQLiteExceptionDelegate;
            _exceptionDelegate = exceptionDelegate;
            _memorycache = memoryCache;

            Task.Run(async () => await Inithializate()).ConfigureAwait(false);
        }

        public async Task Inithializate()
        {
            if (_Is_chekedindex) return;

            if (_Is_chekedindex == false)
            {
                await IndeCreate();
                await IndexProverka();
            }

            _Is_chekedindex = true; 
        }

        public async Task<List<UserContact>> Rquest(string Username)
        {
            _logger.LogWarning("Начинаю запрос в бд");
            _logger.LogWarning(Username);
            SQLiteConnection connection = null;
            SQLiteTransaction transaction = null;
            List<UserContact> userContactlist = new List<UserContact>();
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();
                transaction = connection.BeginTransaction();

                string command = "SELECT * FROM ContactUserBD WHERE Name = @U";

                await using (var sqlcommand = new SQLiteCommand(command, connection, transaction))
                {
                    _logger.LogWarning("команжа выполнена");
                    sqlcommand.Parameters.AddWithValue("@U", Username);
                    _logger.LogWarning("команжа выполнена2");
                    var result = await sqlcommand.ExecuteReaderAsync().ConfigureAwait(false);
                    _logger.LogWarning("команжа выполнена3");

                    while (await result.ReadAsync())
                        {
                        string contactName = result.IsDBNull(2) ? "" : result.GetString(2);
                        _logger.LogWarning($"Имя контакта (индекс 2): '{contactName}'");

                        string yourName = result.IsDBNull(1) ? "" : result.GetString(1);
                        _logger.LogWarning($"Ваше имя (индекс 1): '{yourName}'");

                        string photo = result.IsDBNull(3) ? "" : result.GetString(3);

                        UserContact userContact = new UserContact()
                        {
                            Username = contactName,
                            Name = yourName,  
                            photo = photo
                        };
                        userContactlist.Add(userContact);
                        }

                }
                await transaction.CommitAsync().ConfigureAwait(false);
                return userContactlist;
            }
            catch (SQLiteException ex)
            {
                _logger.LogWarning("1");
                await _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex);
                await (transaction?.RollbackAsync() ?? Task.CompletedTask);
                return new List<UserContact>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("2");
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                await (transaction?.RollbackAsync() ?? Task.CompletedTask);
                return new List<UserContact>();
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

        public async Task IndeCreate()
        {
            SQLiteConnection connection = null;
            try
            {
                connection = _poolSQLiteConnection.ConnectionOpen();

                string command = "CREATE INDEX IF NOT EXISTS IX_ContactUserBD_iNDEX ON ContactUserBD(UserName)";

                await using (var sqlcommand = new SQLiteCommand(command, connection))
                {
                    await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);
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

                string command = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'index' AND name = 'IX_ContactUserBD_iNDEX' AND tbl_name = 'ContactUserBD'";

                await using (var commandsql = new SQLiteCommand(command, connection))
                {
                    var result = await commandsql.ExecuteScalarAsync().ConfigureAwait(false);

                    if (result != null)
                    {
                        bool exec = Convert.ToInt32(result) == 1;

                        if (exec)
                        {
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
                };
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
