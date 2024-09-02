using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.BotService;
using Azure.ResourceManager.BotService.Models;
using Azure;

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

        public async Task<BotResource> CloneBotAsync(string sourceBotName, string newBotName)
        {
            ResourceIdentifier resourceGroupResourceId = ResourceGroupResource.CreateResourceIdentifier(_subscriptionId, _resourceGroupName);
            ResourceGroupResource resourceGroupResource = _armClient.GetResourceGroupResource(resourceGroupResourceId);
            BotCollection collection = resourceGroupResource.GetBots();

            var sourceBot = await collection.GetAsync(sourceBotName);
            var sourceBotData = sourceBot.Value.Data;

            var newBotData = new BotData(sourceBotData.Location)
            {
                Properties = new BotProperties(newBotName, sourceBotData.Properties.Endpoint, Guid.NewGuid().ToString())
                {
                    Description = $"Cloned from {sourceBotName}",
                    DeveloperAppInsightKey = sourceBotData.Properties.DeveloperAppInsightKey,
                    DeveloperAppInsightsApiKey = sourceBotData.Properties.DeveloperAppInsightsApiKey,
                    DeveloperAppInsightsApplicationId = sourceBotData.Properties.DeveloperAppInsightsApplicationId,
                    // Copy other relevant properties
                },
                Sku = sourceBotData.Sku,
                Kind = sourceBotData.Kind,
            };

            var operation = await collection.CreateOrUpdateAsync(WaitUntil.Completed, newBotName, newBotData);
            return operation.Value;
        }

        public async Task<BotChannelResource> CreateChannelAsync(string botName, BotChannelName channelName, object channelData)
        {
            ResourceIdentifier resourceGroupResourceId = ResourceGroupResource.CreateResourceIdentifier(_subscriptionId, _resourceGroupName);
            ResourceGroupResource resourceGroupResource = _armClient.GetResourceGroupResource(resourceGroupResourceId);
            var bot = await resourceGroupResource.GetBotAsync(botName);
            var channelCollection = bot.Value.GetBotChannels();

            var data = new BotChannelData(bot.Value.Data.Location)
            {
                //Properties = channelData,
            };

            var operation = await channelCollection.CreateOrUpdateAsync(WaitUntil.Completed, channelName, data);
            return operation.Value;
        }

        public async Task<string> GetChannelSecretAsync(string botName, BotChannelName channelName)
        {
            ResourceIdentifier resourceGroupResourceId = ResourceGroupResource.CreateResourceIdentifier(_subscriptionId, _resourceGroupName);
            ResourceGroupResource resourceGroupResource = _armClient.GetResourceGroupResource(resourceGroupResourceId);
            var bot = await resourceGroupResource.GetBotAsync(botName);
            var channel = await bot.Value.GetBotChannels().GetAsync(channelName);

            // The actual implementation depends on the channel type and how secrets are stored
            // This is a placeholder and should be implemented based on the specific channel requirements
            return "placeholder_secret";
        }
    }
}