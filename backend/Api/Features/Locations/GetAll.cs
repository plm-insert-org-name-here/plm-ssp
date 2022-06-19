using System.Collections.Generic;
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
    public class GetAll : EndpointBaseAsync.WithoutRequest.WithResult<List<GetAll.LocationResponse>>
    {
        private readonly Context _context;
        private readonly IConfigurationProvider _mapperConfig;

        public GetAll(Context context, IConfigurationProvider mapperConfig)
        {
            _context = context;
            _mapperConfig = mapperConfig;
        }

        public class LocationResponse
        {
            public string Name { get; set; } = default!;
        }

        private class MappingProfile : Profile
        {
            public MappingProfile() => CreateProjection<Location, LocationResponse>();
        }

        [HttpGet(Routes.Locations.GetAll)]
        [SwaggerOperation(
            Summary = "Get all locations",
            Description = "Get all locations",
            OperationId = "Locations.GetAll",
            Tags = new[] { "Locations" })
        ]
        public override Task<List<LocationResponse>> HandleAsync(CancellationToken ct = new())
        {
            return _context.Locations.ProjectTo<LocationResponse>(_mapperConfig).ToListAsync(ct);
        }
    }

}