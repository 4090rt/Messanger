using Microsoft.AspNetCore.Mvc;
using MessangersUI.Delegate;
using Messangers.ModelData;
namespace Messangers.ControllerPost
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostControllerRegister: ControllerBase
    {
        public ILogger<PostControllerRegister> _logger;

        public PostControllerRegister(ILogger<PostControllerRegister> logger)
        {
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] ModelDataRegister model)
        {
            if (model == null)
                return BadRequest(new { error = "Данные не переданы"});

            if (string.IsNullOrEmpty(model.Login))
                return BadRequest(new { error = "Логин обязателен!" });
            if (string.IsNullOrEmpty(model.Password))
                return BadRequest(new { error = "Пароль обязателен!" });

            // запрос к  бд существует ли пользователь с таким ником

            //запрос сохранения в базу данных

            _logger?.LogWarning("Зарегестрирован");
            return Ok(new {message = "Регистрация успешна", username = model.Login});
        }
    }
}
