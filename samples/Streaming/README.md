
# Bot Streaming Example

## Requirements

1. **Direct Line from Azure Bot Service**: Obtain the `DirectLineSecret` from the Direct Line channel in Azure Bot Service. For more details, refer to the documentation at [Azure Bot Service - Direct Line Authentication](https://learn.microsoft.com/en-us/azure/bot-service/rest-api/bot-framework-rest-direct-line-3-0-authentication?view=azure-bot-service-4.0).
2. **Bot Authentication**: Obtain the `MicrosoftAppType`, `MicrosoftAppId`, `MicrosoftAppPassword`, and `MicrosoftAppTenantId` from Azure Bot Service. Refer to the documentation at [Add Authentication to a Bot](https://learn.microsoft.com/en-us/azure/bot-service/bot-builder-authentication?view=azure-bot-service-4).
3. **ngrok or Dev Tunnels**: To redirect the bot's requests to your local machine, use [ngrok](https://ngrok.com/) or [Visual Studio Dev Tunnels](https://learn.microsoft.com/en-us/aspnet/core/test/dev-tunnels?view=aspnetcore-8.0).
4. **Azure OpenAI**: Obtain the `ApiKey`, `Model`, and `Endpoint` from your Azure OpenAI resource.

## Setup

### Configure `appsettings.json`

Edit the `appsettings.json` file with your data.

Example:

```json
{
  "MicrosoftAppType": "MultiTenant",
  "MicrosoftAppId": "a73f0e42-1c7a-4c5b-a92b-9e799e0b1234",
  "MicrosoftAppPassword": "XJq7R~sQ2vLksuabcJH_82H3urcB6xHDeLQNza2x",
  "MicrosoftAppTenantId": "4a2e673b-bf94-4868-b809-6d4be638bf81",
  "AzureOpenAI": {
    "ApiKey": "c1673299b42e50779f8ebc54f8c949bf",
    "Model": "gpt-4o",
    "Endpoint": "https://gpt4otest.azure.com/"
  }
}
```

### Configure `index.html`

Edit the `index.html` file located inside the `wwwroot` folder to add the `DirectLineSecret` in the Authorization header:

```javascript
'Authorization': 'Bearer <YOUR-DIRECTLINE-SECRET-KEY>'
```

## Running Locally

To run the project locally, follow these steps:

1. Start the `Bot.Streaming` project.
2. Configure `ngrok` (or `Dev Tunnels`) with the correct path by running: `ngrok http https://localhost:3970/`.
3. In your Azure Bot, set the `ngrok` URL followed by `/api/messages` (e.g., `https://0000-000-000-000-00.ngrok.app/api/messages`).
4. Interact with your bot and enjoy the streaming!
