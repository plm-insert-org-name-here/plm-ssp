using System.Linq;
using System.Text.Json.Serialization;
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

namespace Application.Features.Detectors
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

        public record Res
        {
            public int Id { get; set; }
            public string Name { get; set; } = default!;
            public string MacAddress { get; set; } = default!;
            public DetectorState State;

            [JsonPropertyName("location")]
            public Location? AttachedLocation { get; set; }

            public record Location(int Id, string Name);
        }

        class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<Location, Res.Location>();
                CreateProjection<Detector, Res>()
                    .ForMember(dest => dest.AttachedLocation, opt => opt.MapFrom(src => src.Location));
            }
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
                .Where(d => d.Id == id)
                .Include(d => d.Location)
                .ProjectTo<Res>(_mapperConfig)
                .SingleOrDefaultAsync(ct);

            if (result is null) return NotFound();

            return result;
        }
    }

}