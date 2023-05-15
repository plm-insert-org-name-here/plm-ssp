using System.IdentityModel.Tokens.Jwt;
using System.Text;
using FastEndpoints;
using Microsoft.IdentityModel.Tokens;

namespace Api.Endpoints.Users;

public class SignOut : EndpointWithoutRequest<SignOut.Res>
{
    public class Res
    {
        public bool Success { get; set; } = default!;
    }
    
    public override void Configure()
    {
        Get(Api.Routes.Users.SignOut);
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
        
        
        HttpContext.Response.Cookies.Append("jwt",token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.Now.AddSeconds(-1)
        });

        var res = new Res
        {
            Success = true
        };

        await SendOkAsync(res, ct);
    }
}