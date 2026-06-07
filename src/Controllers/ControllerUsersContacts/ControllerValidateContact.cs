using Messangers.SQLite.ContactBse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Messangers.Controllers.ControllerUsersContacts
{
    public class RequesValidateContact
    {
        public string user { get; set; }
        public string login { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ControllerValidateContact: ControllerBase
    {
        public ILogger<ControllerValidateContact> _logger;
        public ValidateContact _validatecontact;

        public ControllerValidateContact(ILogger<ControllerValidateContact> logger, ValidateContact validateContact)
        {
            _logger = logger;
            _validatecontact = validateContact;
        }
        [HttpPost("validatecontact")]
        public async Task<IActionResult> ControllerPost([FromBody] RequesValidateContact requesValidateContact)
        {
            string user = requesValidateContact.user;
            string logincontact = requesValidateContact.login;
            if (requesValidateContact != null)
            {
                var result = await _validatecontact.SearchMethod(user,logincontact).ConfigureAwait(false);
                if (result != "" && result == "Успешно")
                {
                    return Ok(new { message = "Пользователь найден в бд", state = "true" });
                }
                return BadRequest(new {message = "не удалось найти данные", state = "false" });
            }
            return BadRequest(new {message = "данные пусты", state = "false"});
        }
    }
}
