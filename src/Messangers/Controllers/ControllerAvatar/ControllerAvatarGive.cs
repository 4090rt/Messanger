using Messangers.SQLite.AvatarAdd;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Messangers.Controllers.ControllerAvatar
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControllerAvatarGive : ControllerBase
    {
        private readonly ILogger<ControllerAvatarGive> _logger;
        private readonly AvatarGive _avatarGive;

        public ControllerAvatarGive(ILogger<ControllerAvatarGive> logger, AvatarGive avatarGive)
        {
            _logger = logger;
            _avatarGive = avatarGive;
        }

        [HttpPost("controllergiveAv")]
        public async Task<IActionResult> ControllerGive([FromBody] string username)
        {
            try
            {
                ReadOnlyMemory<byte> resultbytes = await _avatarGive.CachaRequest(username).ConfigureAwait(false);

                if (resultbytes.Length == 0)
                    return BadRequest(new { Data = new ReadOnlyMemory<byte>(), State = $"Не удалось найти изображение" });

                return Ok(new { Data = resultbytes, State = $"Успешно" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Возникло исключение в контроллере получения аватара" + ex.Message + ex.StackTrace);
                return BadRequest(new { Data = new ReadOnlyMemory<byte>(), State = $"{ex.Message}" });
            }
        }
    }
}
