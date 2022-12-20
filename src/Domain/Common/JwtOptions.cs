namespace Domain.Common;

public class JwtOptions
{
    public const string ConfigurationEntryName = "Jwt";

    public string ValidIssuer { get; set; } = default!;
    public string ValidAudience { get; set; } = default!;
    public int ExpiresInMinutes { get; set; }
    public string Secret { get; set; } = default!;
    public string RoleClaimName { get; set; } = default!;
}