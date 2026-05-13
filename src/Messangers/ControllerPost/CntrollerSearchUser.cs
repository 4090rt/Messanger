using Messangers.SQLite.ContactBse.UserSerach;
using Microsoft.AspNetCore.Mvc;

namespace Messangers.ControllerPost
{
    [ApiController]
    [Route("api/[controller]")]
    public class CntrollerSearchUser: ControllerBase
    {
        private ILogger<CntrollerSearchUser> _logger;
        public Search _search;

        public CntrollerSearchUser(ILogger<CntrollerSearchUser> logger, Search search)
        {
            _logger = logger;
            _search = search;
        }
        [HttpPost("search")]
        public async Task<IActionResult> Controller()
        {
            try
            {
                _logger.LogInformation("Начинаю поиск пользователя для добавления в контакты");
                string base64Data = await new StreamReader(Request.Body).ReadToEndAsync();

                if (base64Data == null)
                {
                    return BadRequest(new {message = "Пустое значение", result = "false"});
                }
                var treamedstring = base64Data.Trim('"');
                var requestinbd = await _search.RequestCache(treamedstring);

                if (requestinbd == true)
                {
                    _logger.LogInformation("ползователь найден!");
                    return Ok(new { message = "Успешно", result = "true" });
                }
                else
                {
                    return BadRequest(new { message = "Пользователь не найден", result = "false" });
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение" + ex.Message + ex.StackTrace + ex.InnerException);
                return BadRequest(new {message = "Возникло имключение", result = "false" });
            }
        }
    }
}
