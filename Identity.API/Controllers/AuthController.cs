using System.Threading.Tasks;

using Identity.API.Jwt;
using Identity.API.Models;

using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService authService;

        public AuthController(AuthService authService)
        {
            this.authService = authService;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            var (token, code) = await this.authService.Authorize(loginModel);

            return this.StatusCode((int)code, token);
        }
    }
}