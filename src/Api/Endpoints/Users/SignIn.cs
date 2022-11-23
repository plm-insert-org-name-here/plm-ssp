using Domain.Entities;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;
using Infrastructure;

namespace Api.Endpoints.Users;

public class SignIn : Endpoint<SignIn.Req, SignIn.Res>
{
    public IRepository<User> UserRepo { get; set; } = default!;
    public class Req
    {
        public string Name { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    public class Res
    {
        public string Status { get; set; } = default!;
        public string Token { get; set; } = default!;
    }
    
    public override void Configure()
    {
        Post(Api.Routes.Users.SignIn);
        AllowAnonymous();
        Options(x => x.WithTags("Users"));
    }

    public override async Task HandleAsync(Req req, CancellationToken ct)
    {
        var user = await UserRepo.FirstOrDefaultAsync(new GetUserByNameSpec(req.Name), ct);

        if (user == null)
        {
            ThrowError("Could not find a user with this name");
            return;
        }

        var res = user.Authenticate(req.Password);
        res.Unwrap();

        var response = new Res
        {
            Status = "Success",
            Token = res.Value
        };

        await SendOkAsync(response, ct);
    }
}