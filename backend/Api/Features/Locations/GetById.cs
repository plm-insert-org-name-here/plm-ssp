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
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Locations
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
            [JsonPropertyName("Detector")]
            public Detector? AttachedDetector { get; set; }

            public record Detector(string Name);
        };

        class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<Detector, Res.Detector>();
                CreateProjection<Location, Res>()
                    .ForMember(dest => dest.AttachedDetector, opt => opt.MapFrom(src => src.Detector));
            }
        }

        [HttpGet(Routes.Locations.GetById, Name = Routes.Locations.GetById)]
        [SwaggerOperation(
            Summary = "Get location by id",
            Description = "Get location by id",
            OperationId = "Locations.GetById",
            Tags = new[] { "Locations" })
        ]
        public override async Task<ActionResult<Res>> HandleAsync(int id, CancellationToken ct = new())
        {
            var result = await _context.Locations
                .Where(l => l.Id == id)
                .Include(l => l.Detector)
                .ProjectTo<Res>(_mapperConfig)
                .SingleOrDefaultAsync(ct);

            if (result is null) return NotFound();

            return result;
        }
    }
}