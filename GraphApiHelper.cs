using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;

public class GraphApiHelper
{
    private readonly GraphServiceClient _graphClient;

    public GraphApiHelper(string clientId, string tenantId, string clientSecret)
    {
        var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        _graphClient = new GraphServiceClient(clientSecretCredential);
    }

    public async Task<(string appId, string appSecret)> CreateAppRegistrationAsync(string displayName)
    {
        var application = new Application
        {
            DisplayName = displayName,
            SignInAudience = "AzureADandPersonalMicrosoftAccount",
            PasswordCredentials =
        [
            new PasswordCredential
                {
                    DisplayName = "Default",
                    EndDateTime = DateTime.UtcNow.AddYears(1)

                }
        ]
        };

        var createdApp = await _graphClient.Applications.PostAsync(application);

        var appId = createdApp.AppId;
        var appSecret = createdApp.PasswordCredentials[0].SecretText;

        return (appId, appSecret);
    }
}