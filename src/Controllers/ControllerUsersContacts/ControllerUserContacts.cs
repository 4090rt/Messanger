using Messangers.SQLite.ContactBse.UserSave;
using MessangersUI.DataModel;
using Microsoft.AspNetCore.Mvc;

namespace Messangers.Controllers.ControllerUsersContacts
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControllerUserContacts: ControllerBase
    {
        public ILogger<ControllerUserContacts> _logger;
        public SaveClass _saveClass;

        public ControllerUserContacts(ILogger<ControllerUserContacts> logger, SaveClass saveClass)
        { 
            _logger = logger;
            _saveClass = saveClass;
        }

        [HttpPost("contact")]
        public async Task<IActionResult> Method([FromBody] List<UserContact> list)
        {
            try
            {
                if (list != null && list.Count > 0)
                {
                    await _saveClass.SaveMethod(list).ConfigureAwait(false);
                    return Ok(new { message = "Готово", state = "true" });
                }
                else
                {
                    return BadRequest(new {message = "Лист пустой", state = "false"});
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Возникло исключение {ex.Message}", state = "false" });
            }
        }
    }
}
