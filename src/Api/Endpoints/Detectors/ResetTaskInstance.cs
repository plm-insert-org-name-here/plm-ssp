using Domain.Common;
using Domain.Entities.CompanyHierarchy;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;

namespace Api.Endpoints.Detectors;

public class ResetTaskInstance : Endpoint<ResetTaskInstance.Req, EmptyResponse>
{
   public IRepository<Location> LocationRepo { get; set; } = default!;

   public class Req
   {
      public int LocationId { get; set; }
   }
   
   public override void Configure()
   {
      Post(Api.Routes.Detectors.ResetTaskInstance);
      AllowAnonymous();
      Options(x => x.WithTags("Detectors"));
   }

   public override async System.Threading.Tasks.Task HandleAsync(Req req, CancellationToken ct)
   {
      var location = await LocationRepo.FirstOrDefaultAsync(new LocationWithOngoingTaskSpec(req.LocationId), ct);

      if (location is null)
      {
         await SendNotFoundAsync(ct);
         return;
      }

      if (location.OngoingTask is null)
      {
         await SendNotFoundAsync(ct);
         return;
      }

      if (location.OngoingTask.OngoingInstance is null)
      {
         await SendNotFoundAsync(ct);
         return;
      }

      location.OngoingTask.OngoingInstance.Abandon();
      location.OngoingTask.OngoingInstance = null;
      // location.OngoingTask.OngoingInstanceId = null;
      location.OngoingTask = null;

      await LocationRepo.SaveChangesAsync(ct);
      await SendNoContentAsync(ct);
   }
}