using Messangers.Delegate;
using Messangers.EthernetRequest;
using Messangers.SQLite.DataBaseCreatesTables.PoolSQLiteConnection;
using Messangers.SQLite.ValidationAndRegistrationUserRequest.RequestRegisterAndLogin;
using MessangersUI.Delegate;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;

namespace Messangers.Controllers.ControllerEthernetStat
{
    [ApiController]
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    public class ControllerGetRequestProvider: ControllerBase
    {
        public ILogger<ControllerGetRequestProvider> _logger;
        public SaveRequestInBdRegister _saveRequestInBdRegister;
        private readonly PoolSQLite _poolSQLiteConnection;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly RequesetInfoProviders _requesetInfoProviders;

        public ControllerGetRequestProvider(ILogger<ControllerGetRequestProvider> logger, SaveRequestInBdRegister saveRequestInBdRegister,
            PoolSQLite poolSQLiteConnection, SQLiteExceptionDelegate sQLiteExceptionDelegate,
            ExceptionDelegate exceptionDelegate, RequesetInfoProviders requesetInfoProviders)
        {
            _logger = logger;
            _saveRequestInBdRegister = saveRequestInBdRegister;
            _poolSQLiteConnection = poolSQLiteConnection;
            _exceptionDelegate = exceptionDelegate;
            _poolSQLiteConnection = poolSQLiteConnection;
            _requesetInfoProviders = requesetInfoProviders;
        }

        [HttpGet("provider")]
        public async Task<IActionResult> Request([FromQuery] string host = "")
        {
            Console.WriteLine("Получение провайдера!!!!!");
            var result = await _requesetInfoProviders.CacheRequest().ConfigureAwait(false);

            if (result == null)
                return BadRequest(new {error = "Не удалось получить результат", state = "error" });

            if (!result.Any())
                return BadRequest(new {error = "Вохвращаемые данные пусты", state = "error"});

            return Ok(new {message = result, state = "succes" });
        }
    }
}
