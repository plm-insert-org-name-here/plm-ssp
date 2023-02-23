using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;

namespace Api.Endpoints.Users;

public class Registrate : Endpoint<Registrate.Req, Registrate.Res>
{
    public IRepository<User> UserRepo { get; set; } = default!;
    public class Req
    {
        public string Name { get; set; } = default!;
        public string Password { get; set; } = default!;
        public UserRole Role { get; set; }
    }

    public class Res
    {
        public int Id { get; set; }
        public string ErrorMessage { get; set; }
    }

    public override void Configure()
    {
        Post(Api.Routes.Users.Create);
        AllowAnonymous();
        // Roles("SuperUser");
        Options(x => x.WithTags("Users"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var isASame = await UserRepo.AnyAsync(new UserNameUniquenessCheckerSpec(req.Name), ct);

        if (isASame)
        {
            var errorRes = new Res()
            {
                Id = 0,
                ErrorMessage = "This name is already taken!"
            };
            
            // await SendNotFoundAsync(ct);
            await SendAsync(errorRes);
            return;
        }
        
        try
        {
            var user = new User(req.Name, req.Password, req.Role);
            await UserRepo.AddAsync(user, ct);

            await UserRepo.SaveChangesAsync(ct);

            var res = new Res
            {
                Id = user.Id
            };
            
            await SendOkAsync(res ,ct);
        }
        catch(Exception exception)
        {
            ThrowError((exception.Message));
            // ThrowError("Could not register the user");
        }

        
    }

}
