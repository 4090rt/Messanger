using Messangers.ModelData;
using MessangersUI.Delegate;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
namespace Messangers.ControllerPost
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostControllerLogin: ControllerBase
    {
        public ILogger<PostControllerLogin> _logger;
        public ExceptionDelegate _exceptionDelegate;
        public TaskCanccelException _canccelException;

        public PostControllerLogin (ILogger<PostControllerLogin> logger, ExceptionDelegate exceptionDelegate, TaskCanccelException canccelException)
        {
            _logger = logger;
            _exceptionDelegate = exceptionDelegate;
            _canccelException = canccelException;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] ModelDataLogin request)
        {
            if (request != null)
                return BadRequest(new {error =  "Данные не переданы" });
            if (string.IsNullOrEmpty(request.Login))
                return BadRequest(new {error = "Логин обязателен!" });
            if (string.IsNullOrEmpty(request.Password))
                return BadRequest(new { error = "Пароль обязателен обязателен!" });

            //Поиск юзера в бд не нашли = null

            //кэширование пароля что передали и сравнение с тем что в бд

            //генерация jwt токена
            _logger.LogWarning("Зарегестрирован");
            return Ok(new {/*token = "",*/ username = request.Login});

        }
    }
}
