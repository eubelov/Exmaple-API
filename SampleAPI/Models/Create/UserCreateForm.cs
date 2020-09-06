using System.ComponentModel.DataAnnotations;

namespace SampleAPI.Models.Create
{
    public class UserCreateForm
    {
        [Required(AllowEmptyStrings = false)]
        [MaxLength(150)]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(150)]
        public string PhoneNumber { get; set; }
    }
}