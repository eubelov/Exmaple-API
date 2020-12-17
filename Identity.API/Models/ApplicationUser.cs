using Microsoft.AspNetCore.Identity;

namespace Identity.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Address { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}