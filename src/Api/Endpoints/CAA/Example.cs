using Api.Endpoints.Detectors;
using Domain.Common.DetectorCommand;
using Domain.Entities;
using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Services;
using Domain.Specifications;
using FastEndpoints;
using Infrastructure;

namespace Api.Endpoints.CAA;

public class Example : EndpointWithoutRequest<bool>
{
    
    public DetectorCommandService CommandService { get; set; } = default!;
    public IRepository<Detector> DetectorRepo { get; set; } = default!;
    public IRepository<Location> LocationRepo { get; set; } = default!;
    public INotifyChannel NotifyChannel { get; set; } = default!;
    
    public class Res
    {
        public bool Success { get; set; }
    }

    public override void Configure()
    {
        Get(Api.Routes.CAA.Example);
        AllowAnonymous();
        Options(x => x.WithTags("CAA"));
    }


    public override async Task HandleAsync(CancellationToken ct)
    {
        var command = new DetectorCommand.StartDetection(5);
        var location = await LocationRepo.FirstOrDefaultAsync(new LocationWithDetectorSpec(3)); 
        // var detector = await DetectorRepo.GetByIdAsync(4);

        if (location is null)
        {
            ThrowError("Endpoint Example: location is not found");
        }

        try
        {
            var result = await CommandService.HandleCommand(location.Detector, command, ct);
            result.Unwrap();

            await DetectorRepo.SaveChangesAsync(ct);
        
            //SSE
            NotifyChannel.AddNotify(location.Id);
            await SendOkAsync(true, ct);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await SendOkAsync(false);
        }
        
    }
}