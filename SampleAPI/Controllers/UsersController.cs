using System;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using SampleAPI.Auth;
using SampleAPI.Ext;
using SampleAPI.Models.Common;
using SampleAPI.Models.Create;
using SampleAPI.Models.Update;
using SampleAPI.Models.View;

#pragma warning disable 1998

namespace SampleAPI.Controllers
{
    /// <summary>
    /// Provides users management capabilities.
    /// </summary>
    [Route("users")]
    [ApiController]
    [Authorize(Roles = Policies.User)]
    [Produces("application/json")]
    public sealed class UsersController : ApplicationControllerBase
    {
        private readonly LinkGenerator linkGenerator;

        public UsersController(IMapper mapper, IMediator mediator, LinkGenerator linkGenerator)
            : base(mapper, mediator)
        {
            this.linkGenerator = linkGenerator;
        }

        /// <summary>
        /// Gets current user's profile.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /users
        ///
        /// </remarks>
        /// <returns>A newly created employee.</returns>
        /// <response code="200">Profile of the current user.</response>
        [HttpGet("", Name = nameof(Get))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces(typeof(UserProfileViewModel))]
        public async Task<IActionResult> Get()
        {
            var profile = new UserProfileViewModel
            {
                Id = Guid.NewGuid()
            }.CreateLinksForUser(this.HttpContext, this.linkGenerator);

            return this.Ok(profile);
        }

        /// <summary>
        /// Gets user's profile by its ID.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /users/2F2FBF7C-3EBD-4B41-BAC1-7B70EA9554A3
        ///
        /// </remarks>
        /// <param name="userId">User's profile.</param>
        /// <returns>Profile of the user.</returns>
        /// <response code="200">Profile of the requested user.</response>
        /// <response code="404">User with provided ID doesn't not exist.</response>
        [HttpGet("{userId:guid}", Name = nameof(GetById))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces(typeof(UserProfileViewModel))]
        public async Task<IActionResult> GetById(Guid userId)
        {
            var profile = new UserProfileViewModel
            {
                Id = userId
            }.CreateLinksForUser(this.HttpContext, this.linkGenerator);

            return this.Ok(profile);
        }

        /// <summary>
        /// Updates current user's profile.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /users
        ///     {
        ///         "phoneNumber": "18008945454"
        ///     }
        ///
        /// </remarks>
        /// <param name="updateForm">User profile's update model.</param>
        /// <returns>Updated user's profile.</returns>
        /// <response code="200">Profile of the user was updated.</response>
        /// <response code="400">Some of the supplied properties are invalid.</response>
        [HttpPut("", Name = nameof(Update))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Produces(typeof(UserProfileViewModel))]
        public async Task<IActionResult> Update(UserProfileUpdateForm updateForm)
        {
            var profile = new UserProfileViewModel
            {
                Id = Guid.NewGuid(),
                PhoneNumber = updateForm.PhoneNumber,
                UserName = "TestUser"
            }.CreateLinksForUser(this.HttpContext, this.linkGenerator);

            return this.Ok(profile);
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /users
        ///     {
        ///         "name": "Super User",
        ///         "phoneNumber" : "+19854764875"
        ///     }
        ///
        /// </remarks>
        /// <param name="createForm">User creation form.</param>
        /// <returns>A newly created user.</returns>
        /// <response code="201">User created successfully.</response>
        /// <response code="400">Some of the supplied properties are invalid.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = Policies.Admin)]
        [Produces(typeof(UserProfileViewModel))]
        public async Task<IActionResult> Create(UserCreateForm createForm)
        {
            var profile = new UserProfileViewModel
            {
                Id = Guid.NewGuid(),
                PhoneNumber = createForm.PhoneNumber,
                UserName = createForm.Name
            }.CreateLinksForUser(this.HttpContext, this.linkGenerator);

            return this.CreatedAtAction(
                nameof(this.Get),
                new { userId = profile.Id },
                profile);
        }

        /// <summary>
        /// Gets the list of user's subscriptions.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /users/2F2FBF7C-3EBD-4B41-BAC1-7B70EA9554A3?page=2&pageSize=20
        ///
        /// </remarks>
        /// <param name="userId">ID of the user.</param>
        /// <param name="page">Number of the page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns>List of user's subscriptions.</returns>
        /// <response code="200">The list of user's subscriptions.</response>
        /// <response code="404">User with provided ID doesn't not exist.</response>
        [HttpGet("{userId:guid}/subscriptions", Name = nameof(GetSubscriptions))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces(typeof(PagedResult<SubscriptionViewModel>))]
        public async Task<IActionResult> GetSubscriptions(Guid userId, [FromQuery] uint page = 1, [FromQuery] uint pageSize = 20)
        {
            var result = new PagedResult<SubscriptionViewModel>
            {
                Data = new[]
                           {
                               new SubscriptionViewModel
                                   {
                                       Id = Guid.NewGuid(),
                                       UserId = userId
                                   },
                               new SubscriptionViewModel
                                   {
                                       Id = Guid.NewGuid(),
                                       UserId = userId
                                   }
                           },
                Page = page,
                PageSize = pageSize
            };

            result.Data = result.Data.Select(x => x.CreateLinksForSubscription(this.HttpContext, this.linkGenerator)).ToArray();

            return this.Ok(result);
        }

        /// <summary>
        /// Deletes the user.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE /users/06876152-2CEA-4750-8584-1D710921C7E7
        ///
        /// </remarks>
        /// <param name="userId">ID of the user to delete.</param>
        /// <response code="204">User deleted successfully.</response>
        /// <response code="404">User does not exist.</response>
        [HttpDelete("{organizationId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = Policies.Admin)]
        public async Task<IActionResult> Delete(Guid userId)
        {
            return this.NoContent();
        }
    }
}