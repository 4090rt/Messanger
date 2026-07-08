using Messangers.Delegate;
using Messangers.SQLite.PoolSQLiteConnection;
using MessangersUI.Delegate;
using System.Data.SQLite;

namespace Messangers.SQLite.HistroyMessage.HistoryAttachment
{
    public class SelectAllPathAttachment
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<SelectAllPathAttachment> _loggr;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly PoolSQLite _poolSQLite;

        public SelectAllPathAttachment(IWebHostEnvironment env, ILogger<SelectAllPathAttachment> loggr, ExceptionDelegate exceptionDelegate, SQLiteExceptionDelegate sQLiteExceptionDelegate,PoolSQLite poolSQLite)
        {
            _env = env;
            _loggr = loggr;
            _exceptionDelegate = exceptionDelegate;
            _sQLiteExceptionDelegate = sQLiteExceptionDelegate;
            _poolSQLite = poolSQLite;
        }

        public async Task<string> FullpathGive(int id)
        {
            SQLiteConnection connection = null;
            try
            {
                connection = _poolSQLite.ConnectionOpen();

                string command = "SELECT FilePath FROM Attachments WHERE Id = @Id";

                await using (var sqlcommand = new SQLiteCommand(command, connection))
                { 
                    sqlcommand.Parameters.AddWithValue("@Id", id);

                    var result = await sqlcommand.ExecuteScalarAsync().ConfigureAwait(false);
                    string rawPath = result?.ToString();

                    if (string.IsNullOrEmpty(rawPath))
                        return null;

                    string cleanPath = rawPath.Trim().Replace(" ", "");

                    string fullPath = Path.Combine(_env.ContentRootPath, cleanPath);

                    return fullPath;
                }
            }
            catch (SQLiteException ex)
            {
                await _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex);
                return "Empty string";
            }
            catch (Exception ex)
            {
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return "Empty string";
            }
            finally
            {
                if (connection != null)
                { 
                    _poolSQLite.CloseConnection(connection);
                }
            }
        }
    }
}
