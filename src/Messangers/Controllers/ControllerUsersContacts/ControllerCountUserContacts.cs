using Messangers.SQLite.ContactBse.CountOfUserVidget;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Messangers.Controllers.ControllerUsersContacts
{
    public class UserModel
    { 
        public string username { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ControllerCountUserContacts:ControllerBase
    {
        public ILogger<ControllerCountUserContacts> _logger;
        public CountUser _countUser;

        public ControllerCountUserContacts(ILogger<ControllerCountUserContacts> logger, CountUser countUser)
        {
            _logger = logger;
            _countUser = countUser;
        }

        [HttpPost("countcontactsvidget")]
        public async Task<IActionResult> RequestController([FromBody] UserModel userModel)
        {
            string username = userModel.username;
            if (userModel != null && !string.IsNullOrEmpty(username))
            {
                int result = await _countUser.Count(username).ConfigureAwait(false);
                if (result > 0)
                {
                    return Ok(new { message = $"Успешно найдено количество контактов у {username}", count = $"{result}", state = "true" });
                }
                return BadRequest(new { message = "Не найдено или нет контактов", count = "0", state = "false" });
            }
            return BadRequest(new { message = "Данные пусты",count = "0", state = "false" });
        }
    }
}
