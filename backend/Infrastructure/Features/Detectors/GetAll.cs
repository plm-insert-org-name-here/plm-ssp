using System.Collections.Generic;
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
    public class GetAll : EndpointBaseAsync
        .WithoutRequest
        .WithResult<List<GetAll.Res>>
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
            public string MacAddress { get; set; } = default!;
            public DetectorState State;

            [JsonPropertyName("location")]
            public Location? AttachedLocation { get; set; }

            public record Location(int Id, string Name);
        }


        private class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<Location, Res.Location>();
                CreateProjection<Detector, Res>()
                    .ForMember(dest => dest.AttachedLocation, opt => opt.MapFrom(src => src.Location)) ;
            }
        }

        [HttpGet(Routes.Detectors.GetAll)]
        [SwaggerOperation(
            Summary = "Get all detectors",
            Description = "Get all detectors",
            OperationId = "Detectors.GetAll",
            Tags = new[] { "Detectors" })
        ]
        public override Task<List<Res>> HandleAsync(CancellationToken ct = new())
        {
            return _context.Detectors
                .Include(d => d.Location)
                .ProjectTo<Res>(_mapperConfig).ToListAsync(ct);
        }
    }
}