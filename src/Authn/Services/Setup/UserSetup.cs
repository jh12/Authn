using OpenIddict.Abstractions;
using Permissions = OpenIddict.Abstractions.OpenIddictConstants.Permissions;

namespace Authn.Services.Setup;

public class UserSetup : IHostedService
{
    private readonly IOpenIddictApplicationManager _manager;

    public UserSetup(IOpenIddictApplicationManager manager)
    {
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await CreateRootUserAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task CreateRootUserAsync()
    {
        if (await _manager.FindByClientIdAsync("root") is null)
        {
            await _manager.CreateAsync(new OpenIddictApplicationDescriptor()
            {
                ClientId = "root",
                ClientSecret = Guid.NewGuid().ToString("D"),
                DisplayName = "Root User",
                Permissions =
                {
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.ClientCredentials,
                }
            });
        }
    }
}
