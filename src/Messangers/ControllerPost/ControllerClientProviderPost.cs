using Messangers.ModelData;
using Microsoft.AspNetCore.Mvc;

namespace Messangers.ControllerPost
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControllerClientProviderPost: ControllerBase
    {
        private readonly ILogger<ControllerClientProviderPost> _logger;
        public ControllerClientProviderPost(ILogger<ControllerClientProviderPost> logger)
        {
            _logger = logger;
        }
        [HttpPost("provider")]
        public async Task<IActionResult> Controller()
        {
            string base64Data = await new StreamReader(Request.Body).ReadToEndAsync();
            _logger.LogInformation("Получены данные о провайдере от клиента!");
            if (base64Data == null)
            {
                return BadRequest(new { message = "Данные пусты", state = "error" });
            }

            _logger.LogInformation($"Получены данные: {base64Data}");

            if (string.IsNullOrEmpty(base64Data))
            {
                return BadRequest(new { message = "Данные пусты", state = "error" });
            }

            base64Data = base64Data.Trim('"');

            try
            {
                byte[] data = Convert.FromBase64String(base64Data);
                _logger.LogInformation($"Декодировано {data.Length} байт");
                return Ok(new { message = "Успешно", state = "success" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, state = "error" });
            }
        }
    }
}
