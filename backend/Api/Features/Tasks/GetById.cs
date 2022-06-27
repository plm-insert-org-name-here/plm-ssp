using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Infrastructure.Database;
using Ardalis.ApiEndpoints;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Task = Api.Domain.Entities.Task;

namespace Api.Features.Tasks
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

        public record Res(int Id, string Name, bool Ordered);

        private class MappingProfile : Profile
        {
            public MappingProfile() => CreateProjection<Task, Res>();
        }

        [HttpGet(Routes.Tasks.GetById, Name = Routes.Tasks.GetById)]
        [SwaggerOperation(
            Summary = "Get task by id",
            Description = "Get task by id",
            OperationId = "Tasks.GetById",
            Tags = new[] { "Tasks" })
        ]
        public override async Task<ActionResult<Res>> HandleAsync(int id, CancellationToken ct = new())
        {
            var result = await _context.Tasks
                .Where(t => t.Id == id)
                .ProjectTo<Res>(_mapperConfig)
                .SingleOrDefaultAsync(ct);

            if (result is null) return NotFound();

            return result;
        }
    }
}