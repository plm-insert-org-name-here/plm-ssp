namespace Infrastructure.Database;

public class DbOpt
{
    public static string SectionName = "Database";

    public string ConnectionString { get; set; } = default!;
    public string SeedFolderRelativePath { get; set; } = default!;
    public bool DeleteFirst { get; set; }
}