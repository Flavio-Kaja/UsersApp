namespace UserService.Configurations;

public class ConnectionStringOptions
{
    public const string SectionName = "ConnectionStrings";
    public const string UserServiceKey = nameof(UserService);

    public string UserService { get; set; } = String.Empty;
}

public static class ConnectionStringOptionsExtensions
{
    public static ConnectionStringOptions GetConnectionStringOptions(this IConfiguration configuration)
        => configuration.GetSection(ConnectionStringOptions.SectionName).Get<ConnectionStringOptions>();
}