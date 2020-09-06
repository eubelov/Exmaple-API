using System;

namespace SampleAPI.Models.View
{
    public class SubscriptionViewModel : ViewModelBase
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public DateTimeOffset SubscribedAt { get; set; }
    }
}