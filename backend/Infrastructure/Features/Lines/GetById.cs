using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Infrastructure.Database;
using Ardalis.ApiEndpoints;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Features.Lines
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
            public List<Station> Stations { get; set; } = default!;

            public record Station(int Id, string Name);
        }

        private class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateProjection<Line, Res>();
                CreateProjection<Station, Res.Station>();
            }

        }

        [HttpGet(Routes.Lines.GetById, Name = Routes.Lines.GetById)]
        [SwaggerOperation(
            Summary = "Get Line by id",
            Description = "Get Line by id",
            OperationId = "Lines.GetById",
            Tags = new[] { "Lines"}
            )]
        public override async Task<ActionResult<Res>> HandleAsync(int id, CancellationToken ct = new())
        {
            var result = await _context.Lines
                .Where(l => l.Id == id)
                .Include(o => o.Stations)
                .ProjectTo<Res>(_mapperConfig)
                .SingleOrDefaultAsync(ct);

            if (result is null) return NotFound();

            return result;
        }
    }
}