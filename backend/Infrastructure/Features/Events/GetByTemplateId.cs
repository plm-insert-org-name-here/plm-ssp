using System;
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

namespace Application.Features.Events
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

        public record Res(int Id, DateTime TimeStamp, Step? StateChange);

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
            var result = await _context.Templates
                .Where(t => t.Id == id)
                .Include(t => t.StateChanges)
                .ThenInclude(c => c.Events)
                .SelectMany(t => t.StateChanges)
                .SelectMany(c => c.Events)
                .ProjectTo<Res>(_mapperConfig)
                .ToListAsync(ct);

            if (result is null) return NotFound();

            return result;
        }
    }
}