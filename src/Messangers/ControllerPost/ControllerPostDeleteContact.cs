using Messangers.SQLite.ContactBse.DeleteContact;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Messangers.ControllerPost
{
    public class RequestDelete
    {
        public string user { get; set; }
        public string login { get; set; }
    }
    [ApiController]
    [Route("api/[controller]")]
    public class ControllerPostDeleteContact: ControllerBase
    {
        public ILogger<ControllerPostDeleteContact> _logger;
        public DeleteContact _delecontact;
        public ControllerPostDeleteContact(ILogger<ControllerPostDeleteContact> logger, DeleteContact delecontact)
        { 
            _logger = logger;
            _delecontact = delecontact; 
        }

        [HttpPost("contact")]
        public async Task<IActionResult> HttpController([FromBody] RequestDelete requestDelete)
        {
            string username = requestDelete?.user;
            string login = requestDelete?.login;

            if (username != null && login != null)
            {
                await _delecontact.Request(username, login);
                return Ok(new { message = "Упешно", state = "true" });
            }
            else
            {
                return BadRequest(new {message = "оШИБКА", state = "false " });
            }
        }
    }
}
