namespace Authn.Config;

public class MongoConfiguration
{
    public string ConnectionString { get; init; } = null!;
    public string Database { get; init; } = "authn";
}
