using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;
using Infrastructure;

namespace Api.Endpoints.Stations;

public class CreateValidator : Validator<Create.Req> {

    public CreateValidator()
    {
        RuleFor(x => x.Name).MaximumLength(8);
        RuleFor(x => x.ParentLineId).LessThan(3);
    }
}

public class Create : Endpoint<Create.Req, Create.Res>
{
    public IRepository<Line> LineRepo { get; set; } = default!;
    public ICHNameUniquenessChecker<ICHNodeWithChildren<Station>, Station> NameUniquenessChecker { get; set; } = default!;

    public class Req
    {
        public int ParentLineId { get; set; }
        public string Name { get; set; } = default!;
    }

    public class Res
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
    }

    private static Res MapOut(Station station) => new()
    {
        Id = station.Id,
        Name = station.Name
    };

    public override void Configure()
    {
        Post(Api.Routes.Stations.Create);
        AllowAnonymous();
        Options(x => x.WithTags("Stations"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var line = await LineRepo.FirstOrDefaultAsync(new CHNodeWithChildrenSpec<Line, Station>(req.ParentLineId), ct);

        if (line is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var result = line.AddChildNode(req.Name, NameUniquenessChecker);
        var newStation = result.Unwrap();

        await LineRepo.SaveChangesAsync(ct);

        var res = MapOut(newStation);

        await SendCreatedAtAsync<Create>(new { newStation.Id }, res, null, null, false, ct);
    }
}
