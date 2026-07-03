using Messangers.Delegate;
using Messangers.SQLite.PoolSQLiteConnection;
using MessangersUI.Delegate;
using System.Data.SQLite;

namespace Messangers.SQLite.HistroyMessage.HistoryAttachment
{
    public class SelectAllPathAttachment
    {
        private readonly ILogger<SelectAllPathAttachment> _loggr;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly TaskCanccelException _taskCanccelException;
        private readonly PoolSQLite _poolSQLite;

        public SelectAllPathAttachment(ILogger<SelectAllPathAttachment> loggr, ExceptionDelegate exceptionDelegate, SQLiteExceptionDelegate sQLiteExceptionDelegate, TaskCanccelException taskCanccelException, PoolSQLite poolSQLite)
        {
            _loggr = loggr;
            _exceptionDelegate = exceptionDelegate;
            _sQLiteExceptionDelegate = sQLiteExceptionDelegate;
            _taskCanccelException = taskCanccelException;
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

                    var resultpath = await sqlcommand.ExecuteReaderAsync().ConfigureAwait(false);

                    if (resultpath != null)
                    {
                        var stringPath = resultpath.ToString();
                        return stringPath;
                    }
                    else
                    {
                        _loggr.LogError($"Не удалось получить полный путь для файла {id}");
                        return "Empty string";
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                await _taskCanccelException.RunDelegate(_taskCanccelException.Delegate, ex);
                return "Empty string";
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
