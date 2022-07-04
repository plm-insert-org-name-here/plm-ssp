using System.Collections.Generic;
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

namespace Api.Features.Locations
{
    public class GetAllFree : EndpointBaseAsync.WithoutRequest.WithResult<List<GetAllFree.Res>>
    {
        private readonly Context _context;
        private readonly IConfigurationProvider _mapperConfig;

        public GetAllFree(Context context, IConfigurationProvider mapperConfig)
        {
            _context = context;
            _mapperConfig = mapperConfig;
        }

        public class Res
        {
            public int Id { get; set; }
            public string Name { get; set; } = default!;
        }

        private class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateProjection<Location, Res>();
            }
    }

        [HttpGet(Routes.Locations.GetAllFree)]
        [SwaggerOperation(
            Summary = "Get all free locations",
            Description = "Get all locations without attached detectors",
            OperationId = "Locations.GetAllFree",
            Tags = new[] { "Locations" })
        ]
        public override Task<List<Res>> HandleAsync(CancellationToken ct = new())
        {
            return _context.Locations
                .Include(l => l.Detector)
                .Where(l => l.Detector == null)
                .ProjectTo<Res>(_mapperConfig).ToListAsync(ct);
        }
    }
}