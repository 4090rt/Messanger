using Messangers.ModelData;
using Messangers.SQLite.UserProviderInsert;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace Messangers.Controllers.ControllerEthernetStat
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControllerClientProviderPost: ControllerBase
    {
        private readonly ILogger<ControllerClientProviderPost> _logger;
        public InsertProvider _insertProvider;
        public ControllerClientProviderPost(ILogger<ControllerClientProviderPost> logger, InsertProvider insertProvider)
        {
            _logger = logger;
            _insertProvider = insertProvider;
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
                string datastring = Encoding.UTF8.GetString(data);
                IpIfo list = JsonSerializer.Deserialize<IpIfo>(datastring);
                if (list != null)
                {
                    await _insertProvider.InsertRequest(list);
                    return Ok(new { message = "Успешно", state = "success" });
                }
                else
                {
                    return BadRequest(new { message = "Не удалось обработать данные", state = "error" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, state = "error" });
            }
        }
    }
}
