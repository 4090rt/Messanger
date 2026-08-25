using Messangers.DataModel;
using Messangers.SQLite.PhoneNumberANDMail;
using Microsoft.AspNetCore.Mvc;

namespace Messangers.Controllers.ControllerPhoneMail
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControllerGiveMailPhone:ControllerBase
    {
        private readonly ILogger<ControllerGiveMailPhone> _logger;
        public readonly GivePhoneAndMail _GivePhoneAndMail;

        public ControllerGiveMailPhone(ILogger<ControllerGiveMailPhone> logger, GivePhoneAndMail givePhoneAndMail)
        { 
            _logger = logger;
            _GivePhoneAndMail = givePhoneAndMail;
        }

        [HttpPost("giveMailPhoneControoller")]
        public async Task<IActionResult> ControolerGive([FromBody] string username)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                    return BadRequest(new { ErrorBody = "Входные данные пусты", Status = "BadRequest" });

                MailNumberStrcuct mailNumberStrcuct = await _GivePhoneAndMail.Request(username).ConfigureAwait(false);
                if (mailNumberStrcuct == null || string.IsNullOrEmpty(mailNumberStrcuct.Mail) && string.IsNullOrEmpty(mailNumberStrcuct.Phone))
                    return BadRequest(new { ErrorBody = $"Не удалось найти данные", Status = "No Data" });

                else
                    return Ok(new {Mail = mailNumberStrcuct.Mail, Phone = mailNumberStrcuct.Phone});
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение в  ControllerGiveMailPhone" + ex.Message + ex.StackTrace);
                return BadRequest(new { ErrorBody = $"Возникло исключение во время выполнения запроса  {ex.Message}", Status = "BadRequest" });
            }
        }
    }
}
