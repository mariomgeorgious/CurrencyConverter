using CurrencyConverter.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CurrencyConverter.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiVersion("1.0")]
    public class Auth : Controller
    {
        private readonly IConfiguration _config;

        public Auth(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserModel login)
        {
            IActionResult response = Unauthorized();
            var user = AuthenticateUser(login);

            if (user != null)
            {
                var tokenString = GenerateJwtToken(user);
                response = Ok(new { token = tokenString });
            }

            return response;
        }

        private string GenerateJwtToken(UserModel userInfo)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: new List<Claim>(),
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private UserModel AuthenticateUser(UserModel login)
        {
            UserModel user = null;

            if (login.Username.ToLower() == "admin" && login.Password == "P@ssw0rd")
            {
                user = new UserModel { Username = "Admin" };
            }
            return user;
        }
    }
}
