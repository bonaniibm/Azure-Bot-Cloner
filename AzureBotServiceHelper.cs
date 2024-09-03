using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.BotService;
using Azure.ResourceManager.BotService.Models;
using Azure;
using Azure.Identity;
using Azure.ResourceManager.AppService;

namespace Azure_Bot_Cloner.Services
{
    public class AzureBotServiceHelper(ArmClient armClient, IConfiguration configuration)
    {
        private readonly ArmClient _armClient = armClient;
        private readonly string _subscriptionId = configuration["AzureConfiguration:SubscriptionId"];
        private readonly string _resourceGroupName = configuration["AzureConfiguration:ResourceGroupName"];

        public async Task<IEnumerable<BotResource>> GetBotsAsync()
        {
            ResourceIdentifier resourceGroupResourceId = ResourceGroupResource.CreateResourceIdentifier(_subscriptionId, _resourceGroupName);
            ResourceGroupResource resourceGroupResource = _armClient.GetResourceGroupResource(resourceGroupResourceId);
            BotCollection collection = resourceGroupResource.GetBots();
            string[] botNames = configuration["AzureConfiguration:BotNames"].Split(',');
            var bots = new List<BotResource>();
            foreach (var botName in botNames)
            {
                var bot = await collection.GetAsync(botName);
                if (bot != null)
                {
                    bots.Add(bot);
                }
            }
            return bots;
        }
        public static async Task<WebSiteData> GetWebApp(string name, string subscriptionId, string resourceGroupName, string tenantId, string clientId, string clientSecret)
        {
            TokenCredential cred = new ClientSecretCredential(tenantId, clientId, clientSecret);
            ArmClient client = new(cred);
            ResourceIdentifier resourceGroupResourceId = ResourceGroupResource.CreateResourceIdentifier(subscriptionId, resourceGroupName);
            ResourceGroupResource resourceGroupResource = client.GetResourceGroupResource(resourceGroupResourceId);
            WebSiteCollection collection = resourceGroupResource.GetWebSites();
            WebSiteResource result = await collection.GetAsync(name);
            return result.Data;
        }
        public static async Task<WebSiteData> CloneWebApp(string name, string subscriptionId, string resourceGroupName, WebSiteData sourceWebApp, string tenantId, string clientId, string clientSecret,
            string appId, string appSecret)
        {
            TokenCredential cred = new ClientSecretCredential(tenantId, clientId, clientSecret);
            ArmClient client = new(cred);
            ResourceIdentifier resourceGroupResourceId = ResourceGroupResource.CreateResourceIdentifier(subscriptionId, resourceGroupName);
            ResourceGroupResource resourceGroupResource = client.GetResourceGroupResource(resourceGroupResourceId);
            WebSiteCollection collection = resourceGroupResource.GetWebSites();
            WebSiteData data = new(sourceWebApp.Location)
            {
                CloningInfo = new Azure.ResourceManager.AppService.Models.CloningInfo(sourceWebApp.Id)
                {
                    CanOverwrite = false,
                    CloneCustomHostNames = true,
                    CloneSourceControl = true,
                    SourceWebAppLocation = sourceWebApp.Location,
                    AppSettingsOverrides =
            {
            ["MicrosoftAppId"] = appId,
            ["MicrosoftAppPassword"] = appSecret,
            },
                    ConfigureLoadBalancing = false,

                },
                Kind = "app",
                AppServicePlanId = sourceWebApp.AppServicePlanId
            };
            ArmOperation<WebSiteResource> lro = await collection.CreateOrUpdateAsync(WaitUntil.Completed, name, data);
            WebSiteResource result = lro.Value;

            return result.Data;
        }

        public static async Task<BotData> GetBot(string name, string subscriptionId, string resourceGroupName, string tenantId, string clientId, string clientSecret)
        {
            TokenCredential cred = new ClientSecretCredential(tenantId, clientId, clientSecret);
            ArmClient client = new(cred);
            ResourceIdentifier resourceGroupResourceId = ResourceGroupResource.CreateResourceIdentifier(subscriptionId, resourceGroupName);
            ResourceGroupResource resourceGroupResource = client.GetResourceGroupResource(resourceGroupResourceId);
            BotCollection collection = resourceGroupResource.GetBots();
            string resourceName = name;
            BotResource result = await collection.GetAsync(resourceName);
            return result.Data;
        }

        public static async Task<BotData> CreateBot(string name, string subscriptionId, string resourceGroupName, BotData sourceBotdata, string tenantId, string clientId, string clientSecret, string appId)
        {
            TokenCredential cred = new ClientSecretCredential(tenantId, clientId, clientSecret);
            ArmClient client = new(cred);
            ResourceIdentifier resourceGroupResourceId = ResourceGroupResource.CreateResourceIdentifier(subscriptionId, resourceGroupName);
            ResourceGroupResource resourceGroupResource = client.GetResourceGroupResource(resourceGroupResourceId);
            BotCollection collection = resourceGroupResource.GetBots();
            BotData data = new(sourceBotdata.Location)
            {
                Properties = new BotProperties(name, new Uri($"https://{name}.azurewebsites.net/api/messages"), appId)
                {
                    Description = sourceBotdata.Properties?.Description,
                    IconUri = sourceBotdata.Properties?.IconUri,
                    DeveloperAppInsightKey = sourceBotdata.Properties?.DeveloperAppInsightKey,
                    DeveloperAppInsightsApiKey = sourceBotdata.Properties?.DeveloperAppInsightsApiKey,
                    DeveloperAppInsightsApplicationId = sourceBotdata.Properties?.DeveloperAppInsightsApplicationId,
                    IsCmekEnabled = sourceBotdata.Properties?.IsCmekEnabled,
                    CmekKeyVaultUri = sourceBotdata.Properties?.CmekKeyVaultUri,
                    PublicNetworkAccess = sourceBotdata.Properties?.PublicNetworkAccess,
                    IsLocalAuthDisabled = sourceBotdata.Properties?.IsLocalAuthDisabled,
                    SchemaTransformationVersion = sourceBotdata.Properties?.SchemaTransformationVersion,
                    OpenWithHint = sourceBotdata.Properties?.OpenWithHint
                },
                Sku = sourceBotdata.Sku,
                Kind = sourceBotdata.Kind,
                ETag = sourceBotdata.ETag,
                Tags =
{
new KeyValuePair<string,string>(sourceBotdata.Tags?.FirstOrDefault().Key, sourceBotdata.Tags?.FirstOrDefault().Value)
}
            };
            ArmOperation<BotResource> lro = await collection.CreateOrUpdateAsync(WaitUntil.Completed, name, data);
            BotResource result = lro.Value;
            return result.Data;
        }

        public static async Task<BotChannelData> CreateDirectLineChannel(string botName, string subscriptionId, string resourceGroupName, BotData sourceBotdata, string tenantId, string clientId, string clientSecret)
        {
            TokenCredential cred = new ClientSecretCredential(tenantId, clientId, clientSecret);
            ArmClient client = new(cred);
            string resourceName = botName;
            ResourceIdentifier botResourceId = BotResource.CreateResourceIdentifier(subscriptionId, resourceGroupName, resourceName);
            BotResource bot = client.GetBotResource(botResourceId);
            BotChannelCollection collection = bot.GetBotChannels();
            BotChannelName channelName = BotChannelName.DirectLineChannel;
            BotChannelData data = new(sourceBotdata.Location)
            {
                Properties = new DirectLineChannel()
                {
                    Properties = new DirectLineChannelProperties()
                    {

                    },
                },
            };
            ArmOperation<BotChannelResource> lro = await collection.CreateOrUpdateAsync(WaitUntil.Completed, channelName, data);
            BotChannelResource result = lro.Value;
            return result.Data;
        }

        public static async Task<BotChannelData> CreateWebChatChannel(string botName, string subscriptionId, string resourceGroupName, BotData sourceBotdata, string tenantId, string clientId, string clientSecret)
        {
            TokenCredential cred = new ClientSecretCredential(tenantId, clientId, clientSecret);
            ArmClient client = new(cred);
            string resourceName = botName;
            ResourceIdentifier botResourceId = BotResource.CreateResourceIdentifier(subscriptionId, resourceGroupName, resourceName);
            BotResource bot = client.GetBotResource(botResourceId);
            BotChannelCollection collection = bot.GetBotChannels();
            BotChannelName channelName = BotChannelName.WebChatChannel;
            BotChannelData data = new(sourceBotdata.Location)
            {
                Properties = new WebChatChannel()
                {
                    Properties = new WebChatChannelProperties()
                    {

                    }
                },
            };
            ArmOperation<BotChannelResource> lro = await collection.CreateOrUpdateAsync(WaitUntil.Completed, channelName, data);
            BotChannelResource result = lro.Value;
            return result.Data;
        }

        public static async Task<BotChannelData> CreateAlexaChannel(string botName, string subscriptionId, string resourceGroupName, BotData sourceBotdata, string tenantId, string clientId, string clientSecret, string alexaSkillId)
        {
            TokenCredential cred = new ClientSecretCredential(tenantId, clientId, clientSecret);
            ArmClient client = new(cred);
            string resourceName = botName;
            ResourceIdentifier botResourceId = BotResource.CreateResourceIdentifier(subscriptionId, resourceGroupName, resourceName);
            BotResource bot = client.GetBotResource(botResourceId);
            BotChannelCollection collection = bot.GetBotChannels();
            BotChannelName channelName = BotChannelName.AlexaChannel;
            BotChannelData data = new(sourceBotdata.Location)
            {
                Properties = new AlexaChannel()
                {
                    Properties = new AlexaChannelProperties(alexaSkillId, true)
                },
            };
            ArmOperation<BotChannelResource> lro = await collection.CreateOrUpdateAsync(WaitUntil.Completed, channelName, data);
            BotChannelResource result = lro.Value;
            return result.Data;
        }

        public static async Task<BotChannelData> CreateEmailChannel(string botName, string subscriptionId, string resourceGroupName, BotData sourceBotdata, string tenantId, string clientId, string clientSecret, string emailId, string pwd)
        {
            TokenCredential cred = new ClientSecretCredential(tenantId, clientId, clientSecret);
            ArmClient client = new(cred);
            string resourceName = botName;
            ResourceIdentifier botResourceId = BotResource.CreateResourceIdentifier(subscriptionId, resourceGroupName, resourceName);
            BotResource bot = client.GetBotResource(botResourceId);
            BotChannelCollection collection = bot.GetBotChannels();
            BotChannelName channelName = BotChannelName.EmailChannel;
            BotChannelData data = new(sourceBotdata.Location)
            {
                Properties = new EmailChannel()
                {
                    Properties = new EmailChannelProperties(emailId, true)
                    {
                        Password = pwd
                    }
                },
            };
            ArmOperation<BotChannelResource> lro = await collection.CreateOrUpdateAsync(WaitUntil.Completed, channelName, data);
            BotChannelResource result = lro.Value;
            return result.Data;
        }

        public static async Task<BotChannelData> CreateDirectLineSpeechChannel(string botName, string subscriptionId, string resourceGroupName, BotData sourceBotdata, string tenantId, string clientId, string clientSecret, string cognitiveServiceRegion, string cognitiveServiceSubscriptionKey)
        {
            TokenCredential cred = new ClientSecretCredential(tenantId, clientId, clientSecret);
            ArmClient client = new(cred);
            string resourceName = botName;
            ResourceIdentifier botResourceId = BotResource.CreateResourceIdentifier(subscriptionId, resourceGroupName, resourceName);
            BotResource bot = client.GetBotResource(botResourceId);
            BotChannelCollection collection = bot.GetBotChannels();
            BotChannelName channelName = BotChannelName.DirectLineSpeechChannel;
            BotChannelData data = new(sourceBotdata.Location)
            {
                Properties = new DirectLineSpeechChannel()
                {
                    Properties = new DirectLineSpeechChannelProperties()
                    {
                        CognitiveServiceRegion = cognitiveServiceRegion,
                        CognitiveServiceSubscriptionKey = cognitiveServiceSubscriptionKey,
                        IsEnabled = true
                    }
                },
            };
            ArmOperation<BotChannelResource> lro = await collection.CreateOrUpdateAsync(WaitUntil.Completed, channelName, data);
            BotChannelResource result = lro.Value;
            return result.Data;
        }
        public static async Task<BotChannelData> CreateLineChannel(string botName, string subscriptionId, string resourceGroupName, BotData sourceBotdata, string tenantId, string clientId, string clientSecret, string channelSecret, string channelAccessToken)
        {
            TokenCredential cred = new ClientSecretCredential(tenantId, clientId, clientSecret);
            ArmClient client = new(cred);
            string resourceName = botName;
            ResourceIdentifier botResourceId = BotResource.CreateResourceIdentifier(subscriptionId, resourceGroupName, resourceName);
            BotResource bot = client.GetBotResource(botResourceId);
            BotChannelCollection collection = bot.GetBotChannels();
            BotChannelName channelName = BotChannelName.LineChannel;
            BotChannelData data = new(sourceBotdata.Location)
            {
                Properties = new LineChannel()
                {
                    Properties = new LineChannelProperties(new LineRegistration[]
                                {
                    new LineRegistration()
                    {
                    ChannelSecret = channelSecret,
                    ChannelAccessToken = channelAccessToken
                    }
                                }),
                },
            };
            ArmOperation<BotChannelResource> lro = await collection.CreateOrUpdateAsync(WaitUntil.Completed, channelName, data);
            BotChannelResource result = lro.Value;
            return result.Data;
        }
    }
}