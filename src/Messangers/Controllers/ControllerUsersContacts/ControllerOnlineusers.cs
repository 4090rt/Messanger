using Messangers.ModelData;
using Messangers.SignalSettings.Hubs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Messangers.Controllers.ControllerUsersContacts
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControllerOnlineusers: ControllerBase
    {
        public ILogger<ControllerOnlineusers> _logger;
        public SignalHub _signalhub;

        public ControllerOnlineusers(ILogger<ControllerOnlineusers> logger, SignalHub signal)
        { 
            _logger = logger;
            _signalhub = signal;
        }

        [HttpPost("onlineuser")]
        public async Task<IActionResult> Controller([FromBody] List<DataUsersList> dataUsersLists)
        {
            try
            {
                if (dataUsersLists != null)
                {
                    var result = await _signalhub.UserOnline(dataUsersLists).ConfigureAwait(false);
                    if (result != null)
                    {
                        _logger.LogError($"!!!!!!!!!!!!!!!!{result.Count}");
                        return Ok(new { message = result, state = "true"});
                    }
                    else
                    {
                        return BadRequest(new {message = "Список не найден", state = "false"});
                    }
                }
                return BadRequest(new { message = "Данные пусты", state = "false" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace + ex.InnerException);
                return BadRequest(new { message = "Возникло исключение" + ex.Message, state = "false" });
            }
        }
    }
}
