using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Api.Infrastructure.Database;
using Ardalis.ApiEndpoints;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Task = Api.Domain.Entities.Task;

namespace Api.Features.Tasks
{
    public class GetAll : EndpointBaseAsync
        .WithoutRequest
        .WithResult<List<GetAll.Res>>
    {
        private readonly Context _context;
        private readonly IConfigurationProvider _mapperConfig;

        public GetAll(IConfigurationProvider mapperConfig, Context context)
        {
            _mapperConfig = mapperConfig;
            _context = context;
        }

        public record Res(string Name, bool Ordered);

        private class MappingProfile : Profile
        {
            public MappingProfile() => CreateProjection<Task, Res>();
        }

        [HttpGet(Routes.Tasks.GetAll)]
        [SwaggerOperation(
            Summary = "Get all tasks",
            Description = "Get all tasks",
            OperationId = "Tasks.GetAll",
            Tags = new[] { "Tasks" })
        ]
        public override Task<List<Res>> HandleAsync(CancellationToken ct = new())
        {
            return _context.Tasks.ProjectTo<Res>(_mapperConfig).ToListAsync(ct);
        }
    }
}