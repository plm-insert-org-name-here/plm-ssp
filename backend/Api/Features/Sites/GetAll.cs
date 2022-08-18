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

namespace Api.Features.Sites
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
        }

        private class MappingProfile : Profile
        {
            public MappingProfile() => CreateProjection<Site, Res>();
        }

        [HttpGet(Routes.Sites.GetAll)]
        [SwaggerOperation(
            Summary = "Get all sites",
            Description = "Get all sites",
            OperationId = "Sites.GetAll",
            Tags = new[] { "Sites"}
            )]
        public override async Task<List<Res>> HandleAsync(CancellationToken ct = new())
        {
            return await _context.Sites.ProjectTo<Res>(_mapperConfig).ToListAsync(ct);
        }
    }
}