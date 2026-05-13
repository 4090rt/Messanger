using Messangers.SQLite.ContactBse.UserSearchContact;
using MessangersUI.DataModel;
using Microsoft.AspNetCore.Mvc;

namespace Messangers.ControllerPost
{
    public class UsernameRequest
    {
        public string user { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ControllerPostUserList: ControllerBase
    {
        public ILogger<ControllerPostUserList> _logger;
        public UserSearchContacts _searchContacts;
        public ControllerPostUserList(ILogger<ControllerPostUserList> logger, UserSearchContacts searchContacts)
        { 
            _logger = logger;
            _searchContacts = searchContacts;
        }

        [HttpPost("listcontacts")]
        public async Task<IActionResult> ControllerList([FromBody] UsernameRequest request)
        {
            string username = request?.user;
            List<UserContact> contact = null;
            if (username != null)
            {
                List<UserContact> contacts = await _searchContacts.Rquest(username);
                if (contacts != null)
                {
                    return Ok(contacts);
                }
                else
                {
                    return BadRequest(contact);
                }
            }
            else
            {
                return BadRequest(contact);
            }
        }
    }
}
