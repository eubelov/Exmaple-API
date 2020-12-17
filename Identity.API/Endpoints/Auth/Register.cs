using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

using Identity.API.Jwt;
using Identity.API.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Nelibur.ObjectMapper;

namespace Identity.API.Endpoints.Auth
{
    [AllowAnonymous]
    [Route("api/auth")]
    public class Register : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;

        public Register(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserViewModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Execute([FromBody] RegisterModel registerModel)
        {
            var appUser = new ApplicationUser
            {
                UserName = registerModel.Email,
                Email = registerModel.Email,
                FirstName = registerModel.FirstName,
                LastName = registerModel.LastName,
                Address = registerModel.Address,
            };

            var result = await this.userManager.CreateAsync(appUser, registerModel.Password);
            if (result.Succeeded)
            {
                result = await this.userManager.AddToRoleAsync(appUser, Policies.User);
                if (result.Succeeded)
                {
                    var viewModel = TinyMapper.Map<UserViewModel>(appUser);
                    viewModel.Role = Policies.User;
                    return this.StatusCode(StatusCodes.Status201Created, viewModel);
                }
            }

            foreach (var error in result.Errors)
            {
                this.ModelState.AddModelError(string.Empty, error.Description);
            }

            return this.BadRequest(this.ModelState);
        }
    }

    public class RegisterModel
    {
        [Required(AllowEmptyStrings = false)]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(100)]
        public string Password { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(200)]
        public string Address { get; set; }
    }
}
