using System;
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

namespace Api.Features.Events
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
        public record Res(int Id, DateTime TimeStamp, StateChange? StateChange);

        private class MappingProfile : Profile
        {
            public MappingProfile() => CreateProjection<Event, Res>();
        }

        [HttpGet(Routes.Events.GetAll)]
        [SwaggerOperation(
            Summary = "Get all events",
            Description = "Get all events",
            OperationId = "Events.GetAll",
            Tags = new[] { "Events" })
        ]

        public override async Task<List<Res>> HandleAsync(CancellationToken ct = new())
        {
            return await _context.Events.ProjectTo<Res>(_mapperConfig).ToListAsync(ct);
        }
    }
}