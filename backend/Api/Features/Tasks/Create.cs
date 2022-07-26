using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Domain.Common;
using Api.Domain.Entities;
using Api.Infrastructure.Database;
using Api.Infrastructure.Validation;
using Ardalis.ApiEndpoints;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Task = Api.Domain.Entities.Task;
using TaskStatus = Api.Domain.Common.TaskStatus;

namespace Api.Features.Tasks
{
    public class Create : EndpointBaseAsync
        .WithRequest<Create.Req>
        .WithActionResult<Create.Res>, ITaskCreate
    {
        private readonly Context _context;
        private readonly IMapper _mapper;
        private readonly IValidator<Req> _validator;

        public Create(Context context, IMapper mapper, IValidator<Req> validator)
        {
            _context = context;
            _mapper = mapper;
            _validator = validator;
        }

        public record Req(string Name, bool? Ordered, int JobId);

        public record Res(int Id, string Name, bool Ordered, int JobId);

        public class Validator : AbstractValidator<Req>
        {
            private readonly Context _context;

            public Validator(Context context)
            {
                _context = context;

                RuleFor(t => t.Name).MaximumLength(64).NotEmpty();
                RuleFor(t => t).Must(HaveUniqueNameWithinJob)
                    .WithMessage("'Name' must be unique within the parent Job.");
                RuleFor(t => t.JobId).NotEmpty();
            }

            private bool HaveUniqueNameWithinJob(Req req) =>
                _context.Jobs
                    .Where(j => j.Id == req.JobId)
                    .Include(j => j.Tasks)
                    .SelectMany(j => j.Tasks)
                    .All(t => t.Name != req.Name);
        }

        private class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<Task, Res>();
            }
        }

        [HttpPost(Routes.Tasks.Create)]
        [SwaggerOperation(
            Summary = "Create new task",
            Description = "Create new task",
            OperationId = "Tasks.Create",
            Tags = new[] { "Tasks" })
        ]
        public override async Task<ActionResult<Res>> HandleAsync(Req req, CancellationToken ct = new())
        {
            var validation = await _validator.ValidateToModelStateAsync(req, ModelState, ct);
            if (!validation.IsValid)
                return ValidationProblem();

            var job = await _context.Jobs
                .Where(j => j.Id == req.JobId)
                .Include(j => j.Tasks)
                .SingleOrDefaultAsync(ct);

            if (job is null) return NotFound();

            var qa = job.Type is JobType.QA;

            if (qa && req.Ordered.HasValue)
                return BadRequest("QA tasks cannot set the 'Ordered' field");

            if (!qa && !req.Ordered.HasValue)
                return BadRequest("Kit tasks must set the 'Ordered' field");

            var task = new Task
            {
                Name = req.Name,
                Ordered = qa ? null : req.Ordered!.Value,
                Status = TaskStatus.Inactive,
                Templates = qa ? null : new List<Template>()
            };

            job.Tasks.Add(task);
            await _context.SaveChangesAsync(ct);

            return CreatedAtRoute(Routes.Tasks.GetById, new { task.Id }, _mapper.Map<Res>(task));
        }

        public async Task<ActionResult<Res>> _HandleAsync(Req req)
        {
            return await HandleAsync(req, new CancellationToken());
        }
    }
}