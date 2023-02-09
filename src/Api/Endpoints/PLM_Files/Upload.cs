using Domain.Common;
using Domain.Interfaces;
using FastEndpoints;
using Infrastructure;

namespace Api.Endpoints.PLM_Files;

public class Upload : Endpoint<Upload.Req, EmptyResponse>
{
    public IRepository<PLM_File> FileRepo { get; set; } = default!;
    public class Req
    {
        public string Name { get; set; } = default!;
        public PLM_FileType Type { get; set; } = default!;
    }
    
    public override void Configure()
    {
        Post(Api.Routes.PLM_Files.Upload);
        AllowAnonymous();
        AllowFileUploads();
        Options(x => x.WithTags("Files"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        if (Files.Count == 0)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var newFile = Files[0];

        var uploadedFile = await PLM_File.Create(req.Name, req.Type, FileRepo, newFile);
        
        await FileRepo.AddAsync(uploadedFile.Unwrap());
        await FileRepo.SaveChangesAsync(ct);
        await SendNoContentAsync(ct);
    }
}