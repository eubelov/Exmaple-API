using System;
using System.Threading.Tasks;

using AutoMapper;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using SampleAPI.Extensions;
using SampleAPI.Models.View;

#pragma warning disable 1998

namespace SampleAPI.Controllers
{
    [ApiController]
    [Route("subscriptions")]
    [Produces("application/json")]
    [Authorize]
    public class SubscriptionsController : ApplicationControllerBase
    {
        private readonly LinkGenerator linkGenerator;

        public SubscriptionsController(IMapper mapper, IMediator mediator, LinkGenerator linkGenerator)
            : base(mapper, mediator)
        {
            this.linkGenerator = linkGenerator;
        }

        /// <summary>
        /// Gets user's subscription by its ID.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /2681BDFF-7077-44B9-895C-8CE8B7942CB0
        ///
        /// </remarks>
        /// <param name="subscriptionId">ID of user's subscription.</param>
        /// <returns>User's subscription.</returns>
        /// <response code="200">User's subscription.</response>
        /// <response code="404">Requested resource does not exist.</response>
        [HttpGet("{subscriptionId:guid}", Name = nameof(GetSubscription))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces(typeof(SubscriptionViewModel))]
        public async Task<IActionResult> GetSubscription(Guid subscriptionId)
        {
            var sub = new SubscriptionViewModel
            {
                Id = subscriptionId,
                UserId = Guid.NewGuid()
            }.CreateLinksForSubscription(this.HttpContext, this.linkGenerator);

            return this.Ok(sub);
        }
    }
}
