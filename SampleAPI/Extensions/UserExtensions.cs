using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using SampleAPI.Controllers;
using SampleAPI.Models;
using SampleAPI.Models.View;

namespace SampleAPI.Extensions
{
    public static class UserExtensions
    {
        public static UserProfileViewModel CreateLinksForUser(
            this UserProfileViewModel user,
            HttpContext context,
            LinkGenerator linkGenerator)
        {
            user.Links.Add(
                new LinksModel(
                    linkGenerator.GetPathByAction(context, nameof(UsersController.Get), "Users"),
                    "self",
                    "GET"));

            user.Links.Add(
                new LinksModel(
                    linkGenerator.GetPathByAction(context, nameof(UsersController.GetById), "Users", new { userId = user.Id }),
                    "get_by_id",
                    "GET"));

            user.Links.Add(
                new LinksModel(
                    linkGenerator.GetPathByAction(context, nameof(UsersController.Update), "Users"),
                    "update_user",
                    "PUT"));

            user.Links.Add(
                new LinksModel(
                    linkGenerator.GetPathByAction(
                        context,
                        nameof(UsersController.GetSubscriptions),
                        "Users",
                        new { userId = user.Id, page = 1, pageSize = 20 }),
                    "get_subscriptions",
                    "GET"));

            return user;
        }
    }
}