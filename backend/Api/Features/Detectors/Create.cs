using System.Threading;
using System.Threading.Tasks;
using Api.Domain.Entities;
using Api.Infrastructure.Database;
using Ardalis.ApiEndpoints;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Features.Detectors
{
    public class Create : EndpointBaseAsync
        .WithRequest<Create.Req>
        .WithActionResult<Create.Res>
    {
        private readonly Context _context;
        private readonly IMapper _mapper;

        public Create(Context context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public record Req(string MacAddress);
        public record Res(int Id, string Name, string MacAddress);

        public class Validator : AbstractValidator<Req>
        {
            public Validator()
            {
                RuleFor(d => d.MacAddress).Matches("[0-9a-fA-F]{12}")
                    .WithMessage("'MacAddress' must contain a valid MAC address without the separator colons");
            }
        }

        private class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<Detector, Res>();
            }
        }

        [HttpPost(Routes.Detectors.Create)]
        [SwaggerOperation(
            Summary = "Create new detector",
            Description = "Create new detector",
            OperationId = "Detectors.Create",
            Tags = new[] { "Detectors" })
        ]
        public override async Task<ActionResult<Res>> HandleAsync(Req req, CancellationToken ct = new())
        {
            var detector = new Detector
            {
                Name = req.MacAddress,
                MacAddress = req.MacAddress,
                State = DetectorState.Standby
            };

            await _context.Detectors.AddAsync(detector, ct);
            await _context.SaveChangesAsync(ct);

            return CreatedAtRoute(Routes.Detectors.GetById, new { detector.Id }, _mapper.Map<Res>(detector));

        }
    }
}