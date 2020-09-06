using System.Linq;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [Authorize]
    [Route("validate")]
    [ApiController]
    public class ValidateController : ControllerBase
    {
        [HttpGet]
        public IActionResult AmIOk()
        {
            return this.Ok(this.User.Claims.Select(x => new { x.Type, x.Value }));
        }
    }
}