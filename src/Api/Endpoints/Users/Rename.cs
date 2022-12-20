using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using FastEndpoints;

public class Rename : Endpoint<Rename.Req, EmptyResponse>
{

    public IRepository<User> UserRepo { get; set; } = default!;
    public class Req
    {
        public int Id { get; set; }
		public string NewName { get; set; } = default!;
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

		user.UserName = req.NewName;

		await UserRepo.SaveChangesAsync(ct);	
        await SendNoContentAsync(ct);
    }
}		      
