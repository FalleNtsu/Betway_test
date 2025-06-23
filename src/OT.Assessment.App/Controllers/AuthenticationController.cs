using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OT.Assessment.App.Models.Requests;
using OT.Assessment.App.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OT.Assessment.App.Controllers
{
    /// <summary>
    /// Handles authentication operations for the API.
    /// </summary>
    [ApiController]
    [Route("api/Auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthRepository _authRepository;
        public AuthenticationController(IConfiguration configuration, IAuthRepository authRepository)
        {
            _configuration = configuration;
            _authRepository = authRepository;
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        /// <param name="request">The login request with username and password.</param>
        /// <returns>A JWT token if successful.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequest request)
        {
            bool validUser = await _authRepository.IsValidUserAsync(request.APIKey, request.Password);

            if (!validUser)
                return Unauthorized("Invalid APIKey or Password");

            var token = GenerateJwtToken(request.APIKey.ToString());
            return Ok(new { token });
        }

        /// <summary>
        /// Generates a JWT token using the provided username.
        /// </summary>
        /// <param name="username">The username to include in the token's claims.</param>
        /// <returns>A signed JWT token string.</returns>
        private string GenerateJwtToken(string username)
        {
            var jwtConfig = _configuration.GetSection("Jwt");

            var claims = new[]
            {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "User")
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtConfig["Issuer"],
                audience: jwtConfig["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtConfig["ExpireMinutes"]!)),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
