using System;

namespace SampleAPI.Models.View
{
    public class UserProfileViewModel : ViewModelBase
    {
        public Guid Id { get; set; }

        public string PhoneNumber { get; set; }

        public string UserName { get; set; }
    }
}