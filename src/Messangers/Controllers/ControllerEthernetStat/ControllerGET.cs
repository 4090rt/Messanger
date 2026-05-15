using DirectoryStatistic.Http.ModelData;
using Messangers.Delegate;
using Messangers.EthernetRequest;
using Messangers.SQLite.DataBaseCreatesTables.PoolSQLiteConnection;
using Messangers.SQLite.ValidationAndRegistrationUserRequest.RequestRegisterAndLogin;
using MessangersUI.Delegate;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Messangers.Controllers.ControllerEthernetStat
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControllerGET : ControllerBase
    {
        public ILogger<ControllerGET> _logger;
        public SaveRequestInBdRegister _saveRequestInBdRegister;
        private readonly PoolSQLite _poolSQLiteConnection;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly PingRequest _pingRequest;

        public ControllerGET(ILogger<ControllerGET> logger, SaveRequestInBdRegister saveRequestInBdRegister,
            PoolSQLite poolSQLiteConnection, SQLiteExceptionDelegate sQLiteExceptionDelegate,
            ExceptionDelegate exceptionDelegate, PingRequest pingRequest)
        {
            _logger = logger;
            _saveRequestInBdRegister = saveRequestInBdRegister;
            _poolSQLiteConnection = poolSQLiteConnection;
            _exceptionDelegate = exceptionDelegate;
            _poolSQLiteConnection = poolSQLiteConnection;
            _pingRequest = pingRequest;
        }
        [HttpGet("ping")]
        public async Task<IActionResult> Pingreq([FromQuery] string host = "google.com")
        {
            var result = await _pingRequest.Request(host).ConfigureAwait(false);
            if (result == null)
            {
                return BadRequest(new { error = "не удалось получить даннык", State = "error" });
            }
            if (!result.Any())
            {
                return BadRequest(new { error = "Список с данными пуст", State = "error" });
            }

            return Ok(new {message = result, State = "succes"});
        }
    }
}
