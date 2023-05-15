using Domain.Common;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;

namespace Api.Endpoints.PLM_Files;

public class GetNewestVersion : Endpoint<GetNewestVersion.Req, GetNewestVersion.Res>
{
    public IRepository<PLM_File> FileRepo { get; set; } = default!;
    public class Req
    {
        public string Name { get; set; }
    }

    public class Res
    {
        public int VersionNum { get; set; }
    }
    
    public override void Configure()
    {
        Get(Api.Routes.PLM_Files.GetNewestVersion);
        AllowAnonymous();
        Options(x => x.WithTags("Files"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var file = await FileRepo.FirstOrDefaultAsync(new FileByNameSpec(req.Name), ct);
        
        if (file is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var res = new Res
        {
            VersionNum = file.VersionNumber
        };

        await SendOkAsync(res, ct);
    }
}