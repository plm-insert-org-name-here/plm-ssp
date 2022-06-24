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

namespace Api.Features.Jobs
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

        public record Res(string Name, JobType Type);

        private class MappingProfile : Profile
        {
            public MappingProfile() => CreateProjection<Job, Res>();
        }

        [HttpGet(Routes.Jobs.GetAll)]
        [SwaggerOperation(
            Summary = "Get all jobs",
            Description = "Get all jobs",
            OperationId = "Jobs.GetAll",
            Tags = new[] { "Jobs" })
        ]
        public override Task<List<Res>> HandleAsync(CancellationToken ct = new())
        {
            return _context.Jobs.ProjectTo<Res>(_mapperConfig).ToListAsync(ct);
        }
    }
}