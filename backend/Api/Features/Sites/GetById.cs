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

namespace Api.Features.Sites
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
            [JsonPropertyName("OPUs")]
            public List<OPU> OPUs { get; set; } = default!;

            public record OPU(int Id, string Name);
        }

        private class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateProjection<Site, Res>();
                CreateProjection<OPU, Res.OPU>();
            }

        }

        [HttpGet(Routes.Sites.GetById, Name = Routes.Sites.GetById)]
        [SwaggerOperation(
            Summary = "Get site by id",
            Description = "Get site by id",
            OperationId = "Sites.GetById",
            Tags = new[] { "Sites"}
            )]
        public override async Task<ActionResult<Res>> HandleAsync(int id, CancellationToken ct = new())
        {
            var result = await _context.Sites
                .Where(s => s.Id == id)
                .Include(s => s.OPUs)
                .ProjectTo<Res>(_mapperConfig)
                .SingleOrDefaultAsync(ct);

            if (result is null) return NotFound();

            return result;
        }
    }
}