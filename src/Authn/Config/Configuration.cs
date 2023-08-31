namespace Authn.Config;

public class Configuration
{
    public ServerConfiguration Server { get; init; } = new();
    public LoggingConfiguration Log { get; init; } = new();
    public MongoConfiguration MongoDb { get; init; } = new();
}
