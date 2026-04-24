using Messangers.Delegate;
using Messangers.SQLite.CreateDataBases;
using Messangers.SQLite.PoolSQLiteConnection;
using Messangers.SQLite.RequestRegisterAndLogin;
using MessangersUI.Delegate;
using System.Data.SQLite;

namespace Messangers.SQLite.InithilizateDataBaseCreate
{
    public class Inithializate
    {
        private readonly ILogger<Inithializate> _logger;
        private readonly PoolSQLite _poolSQLiteConnection;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly CreateRegisterBase _createRegisterBase;

        public Inithializate(ILogger<Inithializate> logger, PoolSQLite poolSQLiteConnection,
            SQLiteExceptionDelegate sQLiteExceptionDelegate, ExceptionDelegate exceptionDelegate, CreateRegisterBase createRegisterBase)
        {
            _logger = logger;
            _poolSQLiteConnection = poolSQLiteConnection;
            _sQLiteExceptionDelegate = sQLiteExceptionDelegate;
            _exceptionDelegate = exceptionDelegate;
            _createRegisterBase = createRegisterBase;
        }

        public async Task<bool> MethodCreateBase()
        {
            _logger.LogWarning("Инициализация базы  RegisterBase");
            try
            {
                await _createRegisterBase.pROVERKA();
                _logger.LogWarning("Успешно инициализированна");
                return true;
            }
            catch (SQLiteException ex)
            {
                _logger.LogWarning("Ошибка инициализации:");
                await _sQLiteExceptionDelegate.RunDelegate(_sQLiteExceptionDelegate.Delegate, ex);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Ошибка инициализации:");
                await _exceptionDelegate.RunDelegate(_exceptionDelegate.DelegateException, ex);
                return false;
            }
        }
    }
}
