using Messangers.ModelData;
using Messangers.SQLite.UserLoginCheck;
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
        public CheckUserInBD _checkuser;
        public CheckHashPasswordFromBD _checkhashPassword;

        public PostControllerLogin (ILogger<PostControllerLogin> logger, ExceptionDelegate exceptionDelegate,
            CheckUserInBD checkUserInBD, CheckHashPasswordFromBD checkhashPassword)
        {
            _logger = logger;
            _exceptionDelegate = exceptionDelegate;
            _checkuser = checkUserInBD;
            _checkhashPassword = checkhashPassword;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] ModelDataLogin request)
        {
            Console.WriteLine("Авторизация пользователя", request.Login);
            if (request == null)
                return BadRequest(new {error =  "Данные не переданы" });
            if (string.IsNullOrEmpty(request.Login))
                return BadRequest(new {error = "Логин обязателен!" });
            if (string.IsNullOrEmpty(request.Password))
                return BadRequest(new { error = "Пароль обязателен обязателен!" });

            var result = await _checkuser.RequestLogin(request.Login);
            if (result == false)
            {
                return BadRequest(new { error = "Пользователь не зарегестрирован" });
            }

            var resultcachepas = await _checkhashPassword.PasswordCheckRequest(request.Login, request.Password);
            if (resultcachepas == false)
            {
                return BadRequest(new { error = "Неверный пароль"});
            }

            //генерация jwt токена
            _logger.LogWarning("Авторизирован");
            return Ok(new {/*token = "",*/ username = request.Login});

        }
    }
}
