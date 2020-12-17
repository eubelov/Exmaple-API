using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using Identity.API.Configuration;
using Identity.API.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Identity.API.Endpoints.Auth
{
    [AllowAnonymous]
    [Route("api/auth")]
    public class Login : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;

        private readonly JwtSettings jwtSettings;

        public Login(UserManager<ApplicationUser> userManager, IOptions<JwtSettings> jwtOptions)
        {
            this.userManager = userManager;
            this.jwtSettings = jwtOptions.Value;
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenViewModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Execute(LoginModel loginModel)
        {
            var user = await this.userManager.FindByEmailAsync(loginModel.Email);
            if (user == null)
            {
                return this.NotFound();
            }

            if (await this.userManager.CheckPasswordAsync(user, loginModel.Password))
            {
                var roles = await this.userManager.GetRolesAsync(user);

                return this.Ok(
                    new TokenViewModel
                    {
                        Token = this.GenerateJwtToken(user, roles)
                    });
            }

            return this.NotFound();
        }

        private string GenerateJwtToken(ApplicationUser user, IEnumerable<string> roles)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(this.jwtSettings.SecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = this.jwtSettings.Audience,
                Issuer = this.jwtSettings.Issuer,
                Expires = DateTime.UtcNow.AddDays(14),
                Subject = new ClaimsIdentity(
                    new[]
                        {
                            new Claim("Id", user.Id),
                            new Claim(ClaimTypes.Email, user.Email),
                        }.Union(roles.Select(x => new Claim(ClaimTypes.Role, x)))),
                SigningCredentials = new SigningCredentials(
                                              new SymmetricSecurityKey(key),
                                              SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(100)]
        public string Password { get; set; }
    }

    public class TokenViewModel
    {
        public string Token { get; set; }
    }
}
