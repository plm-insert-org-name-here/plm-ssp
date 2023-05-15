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
        // public string Status { get; set; } = default!;
        public string Message { get; set; } = default!;
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
        
        HttpContext.Response.Cookies.Append("jwt",res.Value, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.Now.AddMinutes(30)
        });

        var response = new Res
        {
            Message = "Success"
        };

        await SendOkAsync(response, ct);
    }
}