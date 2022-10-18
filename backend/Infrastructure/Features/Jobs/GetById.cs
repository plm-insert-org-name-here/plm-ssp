using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Infrastructure.Database;
using Ardalis.ApiEndpoints;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Common;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Features.Jobs
{
    public class GetById : EndpointBaseAsync
        .WithRequest<int>
        .WithActionResult<GetById.Res>
    {
        private readonly Context _context;
        private readonly IConfigurationProvider _mapperConfig;

        public GetById(Context context, IConfigurationProvider mapperConfig)
        {
            _context = context;
            _mapperConfig = mapperConfig;
        }

        public record Res(int Id, string Name, JobType Type);

        private class MappingProfile : Profile
        {
            public MappingProfile() => CreateProjection<Job, Res>();

        }

        [HttpGet(Routes.Jobs.GetById, Name = Routes.Jobs.GetById)]
        [SwaggerOperation(
            Summary = "Get job by id",
            Description = "Get job by id",
            OperationId = "Jobs.GetById",
            Tags = new[] { "Jobs" })
        ]
        public override async Task<ActionResult<Res>> HandleAsync(int id, CancellationToken ct = new())
        {
            var result = await _context.Jobs
                .Where(j => j.Id == id)
                .ProjectTo<Res>(_mapperConfig)
                .SingleOrDefaultAsync(ct);

            if (result is null) return NotFound();

            return result;
        }
    }
}