namespace Domain.Common;

public class JwtOptions
{
    public const string ConfigurationEntryName = "Jwt";

    public string ValidIssuer { get; set; }
    public string ValidAudience { get; set; }
    public int ExpiresInMinutes { get; set; }
    public string Secret { get; set; }
    public string RoleClaimName { get; set; }
}