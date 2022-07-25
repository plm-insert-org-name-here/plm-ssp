using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Domain.Common;
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
    public class GetAllAttachable : EndpointBaseAsync
        .WithoutRequest
        .WithResult<List<GetAllAttachable.Res>>
    {
        private readonly Context _context;
        private readonly IConfigurationProvider _mapperConfig;

        public GetAllAttachable(Context context, IConfigurationProvider mapperConfig)
        {
            _context = context;
            _mapperConfig = mapperConfig;
        }

        public record Res(int Id, string Name, string MacAddress, DetectorState state);

        private class MappingProfile : Profile
        {
            public MappingProfile() => CreateProjection<Detector, Res>();
        }

        [HttpGet(Routes.Detectors.GetAllAttachable)]
        [SwaggerOperation(
            Summary = "Get all attachable detectors",
            Description = "Get all detectors, which are not attached to locations",
            OperationId = "Detectors.GetAllAttachable",
            Tags = new[] { "Detectors" })
        ]
        public override Task<List<Res>> HandleAsync(CancellationToken ct = new())
        {
            return _context.Detectors
                .Include(d => d.Location)
                .Where(d => d.Location == null)
                .ProjectTo<Res>(_mapperConfig).ToListAsync(ct);
        }
    }
}