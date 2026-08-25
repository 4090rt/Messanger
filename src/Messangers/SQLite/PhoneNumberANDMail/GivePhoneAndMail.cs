using Messangers.DataModel;
using Messangers.Delegate;
using Messangers.SQLite.AvatarAdd;
using Messangers.SQLite.PoolSQLiteConnection;
using MessangersUI.Delegate;
using Microsoft.Extensions.Caching.Memory;
using System.Data.SQLite;

namespace Messangers.SQLite.PhoneNumberANDMail
{
    public class GivePhoneAndMail
    {
        private readonly ILogger<GivePhoneAndMail> _logger;
        private readonly PoolSQLite _poolSQLite;
        private readonly IMemoryCache _memoryCache;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly InvalidExcaptionDelegate _invalidExcaptionDelegate;

        private bool _Is_chekedindex = false;

        public MailNumberStrcuct mailNumberStrcuct = null;

        public GivePhoneAndMail(ILogger<GivePhoneAndMail> logger, PoolSQLite poolSQLite, IMemoryCache memoryCache,
    SQLiteExceptionDelegate sQLiteExceptionDelegate, ExceptionDelegate exceptionDelegate, InvalidExcaptionDelegate invalidExcaptionDelegate)
        {
            _logger = logger;
            _poolSQLite = poolSQLite;
            _memoryCache = memoryCache;
            _exceptionDelegate = exceptionDelegate;
            _sQLiteExceptionDelegate = sQLiteExceptionDelegate;
            _invalidExcaptionDelegate = invalidExcaptionDelegate;

            Task.Run(async () => await InithializateIndex());
        }

        public async Task InithializateIndex()
        {
            if (_Is_chekedindex == true) return;

            if (_Is_chekedindex == false)
            {
                await CreateIndex().ConfigureAwait(false);
                bool result = await IndexProverka().ConfigureAwait(false);

                _Is_chekedindex = result;
            }
        }


        public async Task<MailNumberStrcuct> Request(string username)
        {
            SQLiteConnection connection = new SQLiteConnection();
            try
            {
                connection = _poolSQLite.ConnectionOpen();

                string command = "SELECT TNumber, Mail FROM RegisterBase WHERE Login = @L";

                await using (SQLiteCommand sqlcommand = new SQLiteCommand(command, connection))
                {
                    sqlcommand.Parameters.AddWithValue("@L", username);

                    await using var result = await sqlcommand.ExecuteReaderAsync().ConfigureAwait(false);

                    var indexMail = result.GetOrdinal("Mail");
                    var indexPhone = result.GetOrdinal("TNumber");

                    while (await result.ReadAsync().ConfigureAwait(false))
                    {
                       mailNumberStrcuct = new MailNumberStrcuct
                       { 
                            Mail = result.IsDBNull(indexMail) ? "" : result.GetString(indexMail),
                            Phone = result.IsDBNull(indexPhone) ? "" : result.GetString(indexPhone)
                        };
                    }
                    if (mailNumberStrcuct != null)
                        return mailNumberStrcuct;
                    else
                        return new MailNumberStrcuct();
                }
            }
            catch (SQLiteException ex)
            {
                await _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex).ConfigureAwait(false);
                return new MailNumberStrcuct();
            }
            catch (InvalidOperationException ex)
            {
                await _invalidExcaptionDelegate.RunDelegate(ex).ConfigureAwait(false);
                return new MailNumberStrcuct();
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex).ConfigureAwait(false);
                return new MailNumberStrcuct();
            }
            finally
            {
                if (connection != null)
                    _poolSQLite.CloseConnection(connection);
            }
        }

        public async Task CreateIndex()
        {
            SQLiteConnection connection = new SQLiteConnection();
            try
            {
                connection = _poolSQLite.ConnectionOpen();

                string command = "CREATE IF NOT EXISTS IX_ContactUserBD_IndexPhoneMail ON RegisterBase(Login)";

                await using (SQLiteCommand sqlcommand = new SQLiteCommand(command, connection))
                {
                    await sqlcommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }
            catch (SQLiteException ex)
            {
                await _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex).ConfigureAwait(false);
            }
            catch (InvalidOperationException ex)
            {
                await _invalidExcaptionDelegate.RunDelegate(ex).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex).ConfigureAwait(false);
            }
            finally
            {
                if (connection != null)
                    _poolSQLite.CloseConnection(connection);
            }
        }

        public async Task<bool> IndexProverka()
        {
            SQLiteConnection connection = new SQLiteConnection();
            try
            {
                connection = _poolSQLite.ConnectionOpen();

                string command = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'index' AND name = 'IX_ContactUserBD_IndexPhoneMail' AND tbl_name = 'RegisterBase'";

                await using (SQLiteCommand sqlcommand = new SQLiteCommand())
                {
                    var res = sqlcommand.ExecuteScalarAsync().ConfigureAwait(false);
                    bool exec = Convert.ToInt32(res) == 1;
                    if (exec)
                        return true;
                    else
                        return false;
                }
            }
            catch (SQLiteException ex)
            {
                await _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex).ConfigureAwait(false);
                return false;
            }
            catch (InvalidOperationException ex)
            {
                await _invalidExcaptionDelegate.RunDelegate(ex).ConfigureAwait(false);
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
                    _poolSQLite.CloseConnection(connection);
            }
        }
    }
}
