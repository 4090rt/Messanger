using Messangers.SignalSettings.Hubs;
using Microsoft.AspNetCore.Mvc;

namespace Messangers.Controllers.ControllerUsersContacts
{
    public class modelfirstadd()
    { 
        public string Useradding { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ControllerNewUserOnlineFirstAdd: ControllerBase
    {
        public ILogger<ControllerNewUserOnlineFirstAdd> _logger;
        public SignalHub _signalhub;

        public ControllerNewUserOnlineFirstAdd(ILogger<ControllerNewUserOnlineFirstAdd> logger, SignalHub signalHub)
        {
            _logger = logger;
            _signalhub = signalHub;
        }

        [HttpPost("ControolerOnlineFirstAddUser")]
        public async Task<IActionResult> ControolerPost([FromBody] modelfirstadd modelfirstadd)
        {
            _logger.LogError("В контроллере");
            try
            {
                if (modelfirstadd != null && !string.IsNullOrEmpty(modelfirstadd.Useradding))
                {
                    _logger.LogError("запршаиваю юзера" + modelfirstadd.Useradding);
                    bool result = await _signalhub.UserOnlineValidate(modelfirstadd.Useradding);
                    if (result == true)
                    {
                        _logger.LogError("Успешно вернул truecolor");
                        return Ok(new { message = "Успешно", state = "truecolor" });
                    }
                    else
                    {
                        _logger.LogError("Успешно вернул falsecolor");
                        return Ok(new { message = "Успешно", state = "falsecolor" });
                    }
                }
                else
                {
                    return BadRequest(new { message = "Данные пусты", state = "false" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "ex.Message", state = "false" });
            }
        }
    }
}
