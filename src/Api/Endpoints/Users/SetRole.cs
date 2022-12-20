using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using FastEndpoints;

namespace Api.Endpoints.Users;

public class SetRole : Endpoint<SetRole.Req, EmptyResponse>
{
    public IRepository<User> UserRepo { get; set; } = default!;

    public class Req
    {
        public int Id { get; set;}
        public int NewRole { get; set; }
    }

    public override void Configure()
    {
        Post(Api.Routes.Users.SetRole);
        // AllowAnonymous();
        Roles("SuperUser");
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
        
        Console.WriteLine(user.Role);
        
        user.Role = req.NewRole == 0 ? UserRole.SuperUser : UserRole.Operator;
        
        await UserRepo.SaveChangesAsync(ct);
        Console.WriteLine(user.Role);
        await SendNoContentAsync(ct);
    }
}
