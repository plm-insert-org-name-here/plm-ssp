using System.Collections.Generic;
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

namespace Api.Features.Locations
{
    public class GetAll : EndpointBaseAsync.WithoutRequest.WithResult<List<GetAll.Res>>
    {
        private readonly Context _context;
        private readonly IConfigurationProvider _mapperConfig;

        public GetAll(Context context, IConfigurationProvider mapperConfig)
        {
            _context = context;
            _mapperConfig = mapperConfig;
        }

        public class Res
        {
            public int Id { get; set; }
            public string Name { get; set; } = default!;
            [JsonPropertyName("detector")]
            public Detector? AttachedDetector { get; set; }

            public record Detector(int Id, string Name);
        }

        private class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<Detector, Res.Detector>();
                CreateProjection<Location, Res>()
                    .ForMember(dest => dest.AttachedDetector, opt => opt.MapFrom(src => src.Detector));
            }
    }

        [HttpGet(Routes.Locations.GetAll)]
        [SwaggerOperation(
            Summary = "Get all locations",
            Description = "Get all locations",
            OperationId = "Locations.GetAll",
            Tags = new[] { "Locations" })
        ]
        public override Task<List<Res>> HandleAsync(CancellationToken ct = new())
        {
            return _context.Locations
                .Include(l => l.Detector)
                .ProjectTo<Res>(_mapperConfig).ToListAsync(ct);
        }
    }
}