using Api.Features.Detectors;
using Api.Infrastructure.Database;
using Ardalis.ApiEndpoints;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Domain.Entities;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Features.Events
{
    public class GetByTemplateId : EndpointBaseAsync
        .WithRequest<int>
        .WithActionResult<List<GetByTemplateId.Res>>
    {
        private readonly Context _context;
        private readonly IConfigurationProvider _mapperConfig;

        public GetByTemplateId(Context context, IConfigurationProvider mapperConfig)
        {
            _context = context;
            _mapperConfig = mapperConfig;
        }

        public record Res(int Id, DateTime TimeStamp, Template Template);
        
        private class MappingProfile : Profile
        {
            public MappingProfile() => CreateProjection<Event, Res>();

        }
        
        [HttpGet(Routes.Events.GetByTemplateId, Name = Routes.Events.GetByTemplateId)]
        [SwaggerOperation(
            Summary = "Get event by template id",
            Description = "Get event by template id",
            OperationId = "Events.GetByTemplateId",
            Tags = new[] { "Events" })
        ]
        
        public override async Task<ActionResult<List<Res>>> HandleAsync(int id, CancellationToken ct = new())
        {
            var result = await _context.Events
                .Where(j => j.Template.Id == id)
                .ProjectTo<Res>(_mapperConfig)
                .ToListAsync(ct);

            if (result is null) return NotFound();

            return result;
        }
    }
}