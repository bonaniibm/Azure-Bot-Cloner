# Azure Bot Cloner

Azure Bot Cloner is a web application built with ASP.NET Blazor that facilitates the cloning of Azure bots. This tool streamlines the process of duplicating existing bots, configuring channels, and managing bot secrets.

## Features

- List existing Azure bots
- Clone selected bots with customizable settings
- Configure bot channels (DirectLine and WebChat)
- Retrieve and display channel secrets
- Real-time deployment status updates

## Prerequisites

- .NET 8.0 SDK or later
- Azure subscription
- Azure CLI (for local development and testing)

## Setup

1. Clone the repository:
   ```
   git clone https://github.com/yourusername/azure-bot-cloner.git
   cd azure-bot-cloner
   ```

2. Update the `appsettings.json` file with your Azure configuration:
   ```json
   {
     "AzureAd": {
       "Instance": "https://login.microsoftonline.com/",
       "Domain": "yourdomain.onmicrosoft.com",
       "TenantId": "your-tenant-id",
       "ClientId": "your-client-id",
       "ClientSecret": "your-client-secret"
     },
     "AzureConfiguration": {
       "SubscriptionId": "your-subscription-id",
       "ResourceGroupName": "your-resource-group-name"
     }
   }
   ```

3. Install dependencies:
   ```
   dotnet restore
   ```

4. Run the application:
   ```
   dotnet run
   ```

5. Open a web browser and navigate to `https://localhost:5001` (or the port specified in your console output).

## Usage

1. **List Bots**: Navigate to the Bot List page to view all available bots in your Azure resource group.

2. **Clone Bot**: Select a bot from the list and click "Clone" to start the cloning process. Provide a name for the new bot and select the channels you want to configure.

3. **Deployment Status**: Monitor the cloning progress on the Deployment Status page. This page provides real-time updates on each step of the cloning process.

4. **Channel Secrets**: After successful cloning, you can view the channel secrets for the newly created bot. These secrets are necessary for integrating the bot with various platforms.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## Acknowledgments

- Microsoft Azure Bot Framework
- ASP.NET Blazor Team

## Support

If you encounter any issues or have questions, please file an issue on the GitHub issue tracker.
