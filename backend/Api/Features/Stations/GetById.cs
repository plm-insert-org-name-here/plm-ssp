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

namespace Api.Features.Stations
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
            public List<Location> Locations { get; set; } = default!;

            public record Location(int Id, string Name);
        }

        private class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateProjection<Station, Res>();
                CreateProjection<Location, Res.Location>();
            }

        }

        [HttpGet(Routes.Stations.GetById, Name = Routes.Stations.GetById)]
        [SwaggerOperation(
            Summary = "Get Station by id",
            Description = "Get Station by id",
            OperationId = "Stations.GetById",
            Tags = new[] { "Stations"}
            )]
        public override async Task<ActionResult<Res>> HandleAsync(int id, CancellationToken ct = new())
        {
            var result = await _context.Stations
                .Where(l => l.Id == id)
                .Include(o => o.Locations)
                .ProjectTo<Res>(_mapperConfig)
                .SingleOrDefaultAsync(ct);

            if (result is null) return NotFound();

            return result;
        }
    }
}