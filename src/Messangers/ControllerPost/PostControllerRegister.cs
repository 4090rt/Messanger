using Messangers.Delegate;
using Messangers.ModelData;
using Messangers.SQLite.PoolSQLiteConnection;
using Messangers.SQLite.RequestRegisterAndLogin;
using Messangers.SQLite.UserLoginCheck;
using MessangersUI.Delegate;
using Microsoft.AspNetCore.Mvc;
namespace Messangers.ControllerPost
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostControllerRegister: ControllerBase
    {
        public ILogger<PostControllerRegister> _logger;
        public SaveRequestInBdRegister _saveRequestInBdRegister;
        private readonly PoolSQLite _poolSQLiteConnection;
        private readonly SQLiteExceptionDelegate _sQLiteExceptionDelegate;
        private readonly ExceptionDelegate _exceptionDelegate;
        private readonly CheckLogin _checkLogin;

        public PostControllerRegister(ILogger<PostControllerRegister> logger, SaveRequestInBdRegister saveRequestInBdRegister,
            PoolSQLite poolSQLiteConnection, SQLiteExceptionDelegate sQLiteExceptionDelegate,
            ExceptionDelegate exceptionDelegate, CheckLogin checkLogin)
        {
            _logger = logger;
            _saveRequestInBdRegister = saveRequestInBdRegister;
            _poolSQLiteConnection = poolSQLiteConnection;
            _exceptionDelegate = exceptionDelegate;
            _poolSQLiteConnection = poolSQLiteConnection;
            _checkLogin = checkLogin;
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

            var result = await  _checkLogin.RequestForLogin(model.Login);
            if (result == false)
            {
                return BadRequest(new { error = "Пользователь уже зарегестрирован!" });
            }
            

            await _saveRequestInBdRegister.SaveRegisterDataInBd(model);


            _logger?.LogWarning($"Зарегестрирован {model.Login}");
            return Ok(new {message = "Регистрация успешна", username = model.Login});
        }
    }
}
