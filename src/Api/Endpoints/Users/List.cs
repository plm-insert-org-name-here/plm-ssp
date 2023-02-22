using Domain.Entities;
using Domain.Interfaces;
using FastEndpoints;

namespace Api.Endpoints.Users;

public class List : EndpointWithoutRequest<IEnumerable<List.Res>>
{
    public IRepository<User> UserRepo { get; set; } = default!;
    public class Res
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Role { get; set; } = default!;
    }

    private static Res MapOut(User u) =>
        new()
        {
            Id = u.Id,
            Name = u.UserName,
            Role = u.Role.ToString()
        };
    
    public override void Configure()
    {
        Get(Api.Routes.Users.List);
        AllowAnonymous();
        Options(x => x.WithTags("Users"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var users = await UserRepo.ListAsync(ct);

        await SendOkAsync(users.Select(MapOut), ct);
    }
}