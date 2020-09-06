using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using SampleAPI.Controllers;
using SampleAPI.Models;
using SampleAPI.Models.View;

namespace SampleAPI.Ext
{
    public static class SubscriptionExtensions
    {
        public static SubscriptionViewModel CreateLinksForSubscription(
            this SubscriptionViewModel model,
            HttpContext context,
            LinkGenerator linkGenerator)
        {
            model.Links.Add(
                new LinksModel(
                    linkGenerator.GetPathByAction(
                        context,
                        nameof(SubscriptionsController.GetSubscription),
                        "Subscriptions",
                        new { subscriptionId = model.Id }),
                    "self",
                    "GET"));

            model.Links.Add(
                new LinksModel(
                    linkGenerator.GetPathByAction(context, nameof(UsersController.Get), "Users", new { userId = model.UserId }),
                    "get_user",
                    "GET"));

            return model;
        }
    }
}