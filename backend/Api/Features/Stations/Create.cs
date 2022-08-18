using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Domain.Entities;
using Api.Infrastructure.Database;
using Api.Infrastructure.Validation;
using Ardalis.ApiEndpoints;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Features.Stations
{
    public class Create : EndpointBaseAsync
        .WithRequest<Create.Req>
        .WithActionResult<Create.Res>
    {
        private readonly Context _context;
        private readonly IMapper _mapper;
        private readonly IValidator<Req> _validator;

        public record Req(int ParentId, string Name);
        public record Res(int Id, string Name);

        public Create(Context context, IValidator<Req> validator, IMapper mapper)
        {
            _context = context;
            _validator = validator;
            _mapper = mapper;
        }

        public class Validator : AbstractValidator<Req>
        {
            private readonly Context _context;

            public Validator(Context context)
            {
                _context = context;
                
                RuleFor(s => s.Name).MaximumLength(64).NotEmpty();
                RuleFor(s => s).Must(HaveUniqueNameWithinParent).WithMessage("'Name' must be unique within the Line.");
            }

            private bool HaveUniqueNameWithinParent(Req req) =>
                _context.Stations.Where(s => s.LineId == req.ParentId).All(l => l.Name != req.Name);
        }

        private class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<Station, Res>();
            }
        }

        [HttpPost(Routes.Stations.Create)]
        [SwaggerOperation(
            Summary = "Create new Station",
            Description = "Create new Station",
            OperationId = "Stations.Create",
            Tags = new[] { "Stations" })
        ]
        public override async Task<ActionResult<Res>> HandleAsync(Req req, CancellationToken ct = new())
        {
            var validation = await _validator.ValidateToModelStateAsync(req, ModelState, ct);
            if (!validation.IsValid)
                return ValidationProblem();

            var parentLine = await _context.Lines
                .Include(o => o.Stations)
                .SingleOrDefaultAsync(o => o.Id == req.ParentId, ct);

            if (parentLine is null)
                return BadRequest("Parent Line does not exist!");

            var station = new Station
            {
                Name = req.Name,
            };

            parentLine.Stations.Add(station);
            await _context.SaveChangesAsync(ct);

            return CreatedAtRoute(Routes.Locations.GetById, new { station.Id },
                _mapper.Map<Res>(station));
        }
    }
}