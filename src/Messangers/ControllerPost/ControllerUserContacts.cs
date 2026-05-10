using MessangersUI.DataModel;
using Microsoft.AspNetCore.Mvc;

namespace Messangers.ControllerPost
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControllerUserContacts: ControllerBase
    {
        public ILogger<ControllerUserContacts> _logger;

        public ControllerUserContacts(ILogger<ControllerUserContacts> logger)
        { 
            _logger = logger;
        }

        [HttpPost("contact")]
        public async Task<IActionResult> Method([FromBody] List<UserContact> list)
        {
            try
            {
                if (list != null && list.Count > 0)
                {
                    // мето сохранения в бд
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
