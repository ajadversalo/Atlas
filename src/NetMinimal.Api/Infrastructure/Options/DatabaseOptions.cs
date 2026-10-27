using System.ComponentModel.DataAnnotations;

namespace NetMinimal.Infrastructure.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    [Required]
    public DatabaseProvider Provider { get; set; } = DatabaseProvider.Sqlite;

    [Range(1, 300)]
    public int CommandTimeoutSeconds { get; set; } = 30;

    public bool MigrateOnStartup { get; set; } = true;
    public bool SeedOnStartup { get; set; } = true;
    public bool EnableSensitiveDataLogging { get; set; }
    public bool EnableDetailedErrors { get; set; }
}

public enum DatabaseProvider
{
    Sqlite = 0,
    SqlServer = 1
}
