using System.Data.SQLite;

namespace Messangers.Delegate
{
    public delegate Task SQLiteDelegate(SQLiteException ex);
    public class SQLiteExceptionDelegate
    {
        private readonly ILogger<SQLiteExceptionDelegate> _logger;
        public SQLiteExceptionDelegate(ILogger<SQLiteExceptionDelegate> logger)
        {
            _logger = logger;
        }

        public async Task RunDelegate(SQLiteDelegate sQLiteDelegate, SQLiteException ex)
        { 
            await sQLiteDelegate(ex);
        }

        public async Task Delegate(SQLiteException ex)
        {
            _logger.LogError("Возникло необработанное SQLite исключение " + ex.Message, "место: " + ex.StackTrace, "полное исключение " + ex.InnerException);
        }
    }
}
