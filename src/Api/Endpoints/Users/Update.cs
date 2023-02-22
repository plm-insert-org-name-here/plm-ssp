using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using FastEndpoints;
using Infrastructure;
using Microsoft.IdentityModel.Tokens;

public class Update : Endpoint<Update.Req, Update.Res>
{

    public IRepository<User> UserRepo { get; set; } = default!;
    public class Req
    {
        public int Id { get; set; }
		public string? NewName { get; set; } = default!;
        public string? NewPassword { get; set; } = default!;
        public UserRole? NewRole { get; set; } = default!;
    }

    public class Res
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Role { get; set; } = default!;
    }

    public override void Configure()
    {
        Post(Api.Routes.Users.Update);
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

        var result = user.Update(req.NewName, req.NewPassword, req.NewRole);
		user = result.Unwrap();

        await UserRepo.SaveChangesAsync(ct);	
        
        var res = new Res
        {
            Id = user.Id,
            Name = user.UserName,
            Role = user.Role.ToString()
        };
        
        await SendOkAsync(res, ct);
    }
}		      
