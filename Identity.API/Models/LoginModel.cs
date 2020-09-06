using System.ComponentModel.DataAnnotations;

namespace Identity.API.Models
{
    public class LoginModel
    {
        [Required(AllowEmptyStrings = false)]
        [MaxLength(50)]
        public string Login { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(50)]
        public string Password { get; set; }
    }
}
