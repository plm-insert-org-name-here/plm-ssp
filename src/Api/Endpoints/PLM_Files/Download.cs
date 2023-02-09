using Domain.Common;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;

namespace Api.Endpoints.PLM_Files;

public class Download : Endpoint<Download.Req, Download.Res>
{
    public IRepository<PLM_File> FileRepo { get; set; } = default!;
    public class Req
    {
        public string Name { get; set; } = default!;
    }

    public class Res
    {
        public byte[] File { get; set; } = default!;
    }
    
    public override void Configure()
    {
        Get(Api.Routes.PLM_Files.Download);
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
            File = File.ReadAllBytes(file.FilePath)
        };

        await SendOkAsync(res, ct);
    }
}