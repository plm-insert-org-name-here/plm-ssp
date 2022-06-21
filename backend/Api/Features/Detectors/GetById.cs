using System.Linq;
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

namespace Api.Features.Detectors
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

        public record Res(string Name, string MacAddress, DetectorState state);

        class MappingProfile : Profile
        {
            public MappingProfile() => CreateProjection<Detector, Res>();
        }

        [HttpGet(Routes.Detectors.GetById, Name = Routes.Detectors.GetById)]
        [SwaggerOperation(
            Summary = "Get detector by id",
            Description = "Get detector by id",
            OperationId = "Detectors.GetById",
            Tags = new[] { "Detectors" })
        ]
        public override async Task<ActionResult<Res>> HandleAsync(int id, CancellationToken ct = new())
        {
            var result = await _context.Detectors
                .Where(l => l.Id == id)
                .ProjectTo<Res>(_mapperConfig)
                .SingleOrDefaultAsync(ct);

            if (result is null) return NotFound();

            return result;
        }
    }

}