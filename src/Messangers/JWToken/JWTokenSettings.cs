using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Messangers.JWToken
{
    public class JWTokenSettings
    {
        private readonly ILogger<JWTokenSettings> _logger;
        private readonly IConfiguration _configuration;

        public JWTokenSettings(ILogger<JWTokenSettings> logger, IConfiguration configuration)
        { 
            _logger = logger;
            _configuration = configuration;
        }

        public string CreateToken(string UserName, string Role)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.UniqueName, UserName),
                new Claim("role", Role)
            };

            string stringjson = File.ReadAllText("appsettings.json");
            using JsonDocument document = JsonDocument.Parse(stringjson);

            string Key = document.RootElement
            .GetProperty("SecretKey")
            .GetProperty("key")
            .GetString();

            var secretkey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(Key));

            var crendentails = new SigningCredentials(secretkey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
                (
                    issuer: "https://localhost:7167",
                    audience: "Client",
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(24),
                    notBefore: DateTime.UtcNow,
                    signingCredentials: crendentails
                );

            var tokenhandler = new JwtSecurityTokenHandler();
            return tokenhandler.WriteToken(token);
        }
    }
}
