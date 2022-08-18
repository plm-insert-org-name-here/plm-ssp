using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Api.Domain.Entities;
using Api.Infrastructure.Database;
using Ardalis.ApiEndpoints;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Features.OPUs
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

        public class Res
        {
            public int Id { get; set; }
            public string Name { get; set; } = default!;
            public List<Line> Lines { get; set; } = default!;

            public record Line(int Id, string Name);
        }

        private class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateProjection<OPU, Res>();
                CreateProjection<Line, Res.Line>();
            }

        }

        [HttpGet(Routes.OPUs.GetById, Name = Routes.OPUs.GetById)]
        [SwaggerOperation(
            Summary = "Get OPU by id",
            Description = "Get OPU by id",
            OperationId = "OPUs.GetById",
            Tags = new[] { "OPUs"}
            )]
        public override async Task<ActionResult<Res>> HandleAsync(int id, CancellationToken ct = new())
        {
            var result = await _context.OPUs
                .Where(o => o.Id == id)
                .Include(o => o.Lines)
                .ProjectTo<Res>(_mapperConfig)
                .SingleOrDefaultAsync(ct);

            if (result is null) return NotFound();

            return result;
        }
    }
}