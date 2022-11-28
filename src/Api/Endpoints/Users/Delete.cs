using Domain.Entities;
using Domain.Interfaces;
using FastEndpoints;

namespace Api.Endpoints.Users;

public class Delete : Endpoint<Delete.Req, EmptyResponse>
{
    public IRepository<User> UserRepo { get; set; } = default!;
    public class Req
    {
        public int Id { get; set; }
    }
    
    public override void Configure()
    {
        Delete(Api.Routes.Users.Delete);
        AllowAnonymous();
        Options(x => x.WithTags("Users"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var user = await UserRepo.GetByIdAsync(req.Id, ct);
        
        if (user is null)
        {
            await SendNotFoundAsync(ct);
            return;   
        }

        await UserRepo.DeleteAsync(user, ct);

        await SendNoContentAsync(ct);
    }

}