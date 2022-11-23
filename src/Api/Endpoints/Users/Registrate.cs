using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using FastEndpoints;

namespace Api.Endpoints.Users;

public class Registrate : Endpoint<Registrate.Req, EmptyResponse>
{
    public IRepository<User> UserRepo { get; set; } = default!;
    public class Req
    {
        public string Name { get; set; } = default!;
        public string Password { get; set; } = default!;
        public int Role { get; set; }
    }

    public override void Configure()
    {
        Post(Api.Routes.Users.Create);
        AllowAnonymous();
        Options(x => x.WithTags("Users"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var role = req.Role == 0 ? UserRole.Operator : UserRole.SuperUser;
        try
        {
            var user = new User(req.Name, req.Password, role);
            await UserRepo.AddAsync(user, ct);

            await UserRepo.SaveChangesAsync(ct);
        }
        catch(Exception exception)
        {
            ThrowError((exception.Message));
            // ThrowError("Could not register the user");
        }

        await SendOkAsync(ct);
    }

}