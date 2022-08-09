using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Domain.Common;
using Api.Domain.Entities;
using Api.Infrastructure.Database;
using Api.Infrastructure.Validation;
using Api.Services;
using Ardalis.ApiEndpoints;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using Task = Api.Domain.Entities.Task;

namespace Api.Features.Jobs
{
    public class Create : EndpointBaseAsync
        .WithRequest<Create.Req>
        .WithActionResult<Create.Res>
    {
        private readonly Context _context;
        private readonly SnapshotCache _snapshotCache;
        private readonly IMapper _mapper;
        private readonly IValidator<Req> _validator;
        private readonly ILogger _logger;

        public Create(Context context, SnapshotCache snapshotCache, IMapper mapper, IValidator<Req> validator, ILogger logger)
        {
            _context = context;
            _snapshotCache = snapshotCache;
            _mapper = mapper;
            _validator = validator;
            _logger = logger;
        }

        public record Req(string Name, JobType? Type, int LocationId);

        public record Res(int Id, string Name, JobType Type, int LocationId);

        public class Validator : AbstractValidator<Req>
        {
            private readonly Context _context;
            public Validator(Context context)
            {
                _context = context;

                RuleFor(j => j.Name).MaximumLength(64).NotEmpty();
                RuleFor(j => j).Must(HaveUniqueName).WithMessage("'Name' must be unique.");
                // NOTE(rg): NotEmpty would reject the "0" enum value
                RuleFor(j => j.Type).NotNull();
                RuleFor(j => j.LocationId).NotEmpty();
            }

            private bool HaveUniqueName(Req req) =>
                _context.Jobs.All(j => j.Name != req.Name);
        }

        private class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<Job, Res>();
            }
        }

        [HttpPost(Routes.Jobs.Create)]
        [SwaggerOperation(
            Summary = "Create new job",
            Description = "Create new job",
            OperationId = "Jobs.Create",
            Tags = new[] { "Jobs" })
        ]
        public override async Task<ActionResult<Res>> HandleAsync(Req req, CancellationToken ct = new())
        {
            _logger.Warning("Before validation: {@Req}", req);
            var validation = await _validator.ValidateToModelStateAsync(req, ModelState, ct);
            if (!validation.IsValid)
                return ValidationProblem();

            var location = await _context.Locations
                .Where(l => l.Id == req.LocationId)
                .Include(l => l.Job)
                .SingleOrDefaultAsync(ct);


            if (location is null) return NotFound();
            if (location.Job is not null)
                return BadRequest("The specified location already has a Job associated with it") ;

            var snapshot = _snapshotCache.Get(location.Id);
            if (snapshot is null)
                return BadRequest("No snapshot has been yet taken on the location");

            var job = new Job
            {
                Name = req.Name,
                Type = req.Type!.Value,
                Location = location,
                Snapshot = snapshot,
                Tasks = new List<Task>()
            };

            await _context.Jobs.AddAsync(job, ct);
            await _context.SaveChangesAsync(ct);

            return CreatedAtRoute(Routes.Jobs.GetById, new { job.Id }, _mapper.Map<Res>(job));
        }
    }
}