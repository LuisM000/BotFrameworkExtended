using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace Bot.Streaming.Controllers;

[Route("api/messages")]
[ApiController]
public class BotController(IBotFrameworkHttpAdapter adapter, IBot bot) : ControllerBase
{
    [HttpPost]
    [HttpGet]
    public async Task PostAsync()
    {
        // Delegate the processing of the HTTP POST to the adapter.
        // The adapter will invoke the bot.
        await adapter.ProcessAsync(Request, Response, bot);
    }
}
