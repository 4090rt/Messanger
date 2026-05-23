using Messangers.ModelData;
using Messangers.SignalSettings.Hubs;
using Microsoft.AspNetCore.Mvc;

namespace Messangers.Controllers.ControllerUsersContacts
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControllerOnlineUser: ControllerBase
    {
        private readonly ILogger<ControllerOnlineUser> _logger;
        private readonly SignalHub _signalhub;

        public ControllerOnlineUser(ILogger<ControllerOnlineUser> logger, SignalHub signalHub)
        {
            _logger = logger;
            _signalhub = signalHub;
        }

        [HttpPost("onlinetusers")]
        public async Task<IActionResult> Controller([FromBody] List<DataUsersList> lists)
        {
            try
            {
                if (lists != null && lists.Count > 0)
                {
                    List<DataUsersList> dataUsersLists = await _signalhub.UserOnline(lists);
                    if (dataUsersLists != null)
                    { 
                        return Ok(new {message = dataUsersLists, state = "false"});
                    }
                }
                return BadRequest(new {message = "лист пуст", state = "false"});
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace + ex.InnerException);
                return BadRequest(new { message = "Возникло исключение при поптыке запроса онлайн юзеров", state = "false" });
            }
        }
    }
}
