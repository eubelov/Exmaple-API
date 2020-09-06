using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace SampleAPI.Controllers
{
    public abstract class ApplicationControllerBase : ControllerBase
    {
        protected ApplicationControllerBase(IMapper mapper, IMediator mediator)
        {
            this.Mapper = mapper;
            this.Mediator = mediator;
        }

        protected IMapper Mapper { get; }

        protected IMediator Mediator { get; }

        protected Guid UserId
        {
            get
            {
                if (this.User != null && this.User.Claims.Any())
                {
                    return Guid.Parse(this.User.Claims.First(x => x.Type == WellKnownClaims.Id).Value);
                }

                return Guid.Empty;
            }
        }

        protected string UserName
        {
            get
            {
                return this.User?.Claims.FirstOrDefault(x => x.Type == WellKnownClaims.UserName)?.Value;
            }
        }

        protected TDestination Map<TDestination>(object from)
        {
            return this.Mapper.Map<TDestination>(from);
        }

        protected Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            return this.Mediator.Send(request, cancellationToken);
        }
    }
}