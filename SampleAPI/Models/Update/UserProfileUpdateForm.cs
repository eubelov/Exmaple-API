using System.ComponentModel.DataAnnotations;

namespace SampleAPI.Models.Update
{
    public class UserProfileUpdateForm
    {
        [Required(AllowEmptyStrings = false)]
        [Phone]
        public string PhoneNumber { get; set; }
    }
}