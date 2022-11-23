namespace Domain.Common;

public class DefaultUserOptions
{
    public const string ConfigurationEntryName = "DefaultUser";

    public string UserName { get; set; }
    public string Password { get; set; }
}