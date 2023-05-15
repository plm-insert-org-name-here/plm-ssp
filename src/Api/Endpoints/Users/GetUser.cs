using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Specifications;
using FastEndpoints;
using Microsoft.IdentityModel.Tokens;

namespace Api.Endpoints.Users;

public class GetUser : EndpointWithoutRequest<GetUser.Res>
{
    public IRepository<User> UserRepo { get; set; } = default!; 

    public class Res
    {
        public string Name { get; set; } = default!;
        public string Role { get; set; } = default!;
    }
    
    public override void Configure()
    {
        Get(Api.Routes.Users.GetUser);
        AllowAnonymous();
        Options(x => x.WithTags("Users"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var token = HttpContext.Request.Cookies["jwt"];

        if (token is null)
        {
            ThrowError("invalid token");
            return; 
        }
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("StrONGKAutHENTICATIONKEy");
        tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuerSigningKey = true,
            ValidateIssuer = false,
            ValidateAudience = false
        }, out SecurityToken validatedToken);
        
        var user = await UserRepo.GetByIdAsync(int.Parse(validatedToken.Issuer), ct);
        
        if (user is null)
        {
            ThrowError("User does not exist");
            return;
        }
        
        var res = new Res
        {
            Name = user.UserName,
            Role = user.Role.ToString()
        };
        
        await SendOkAsync(res, ct);
    }
}