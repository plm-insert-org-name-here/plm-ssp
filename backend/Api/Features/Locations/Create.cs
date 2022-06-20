using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Domain.Entities;
using Api.Infrastructure.Database;
using Ardalis.ApiEndpoints;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Features.Locations
{
    public class Create : EndpointBaseAsync
        .WithRequest<Create.Req>
        .WithActionResult<Create.Res>
    {
        private readonly Context _context;
        private readonly IMapper _mapper;

        public record Req(string Name);
        public record Res(int Id, string Name);

        public Create(Context context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public class Validator : AbstractValidator<Req>
        {
            private readonly Context _context;
            public Validator(Context context)
            {
                _context = context;

                RuleFor(l => l.Name).MaximumLength(64);
                RuleFor(l => l).Must(HaveUniqueName).WithMessage("'Name' must be unique.");
            }
            private bool HaveUniqueName(Req req) => _context.Locations.All(l => l.Name != req.Name);
        }

        private class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<Location, Res>();
            }
        }

        [HttpPost(Routes.Locations.Create)]
        [SwaggerOperation(
            Summary = "Create new location",
            Description = "Create new location",
            OperationId = "Locations.Create",
            Tags = new[] { "Locations" })
        ]
        public override async Task<ActionResult<Res>> HandleAsync(Req req, CancellationToken ct = new())
        {
            var location = new Location
            {
                Name = req.Name
            };

            await _context.Locations.AddAsync(location, ct);
            await _context.SaveChangesAsync(ct);

            return CreatedAtRoute(Routes.Locations.GetById, new { location.Id }, _mapper.Map<Res>(location));
        }
    }
}