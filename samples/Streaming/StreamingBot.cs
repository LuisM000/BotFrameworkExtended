using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotFrameworkExtended.Streaming.Extensions;
using BotFrameworkExtended.Streaming.Options;

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using OpenAI.Chat;

namespace Bot.Streaming;

public class StreamingBot(ChatClient chatClient, IStorage storage) : ActivityHandler
{
    private static readonly StreamingMessageOptions StreamingOptions = new()
    {
        InitialProcessingDelay = TimeSpan.FromMilliseconds(200),
        NoMessageDelay = TimeSpan.FromMilliseconds(10),
        MessageProcessingDelay = TimeSpan.FromMilliseconds(200),
    };

    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        var chatHistory = await GetHistoryAsync(turnContext, cancellationToken);
        chatHistory.Add(new UserChatMessage(turnContext.Activity.Text));

        var responseStreaming = chatClient.CompleteChatStreamingAsync(chatHistory, cancellationToken: cancellationToken)
            .Where(c => c.ContentUpdate.Count > 0)
            .Select(d => d.ContentUpdate[0].Text);

        var response = await turnContext.SendStreamingMessageAsync(responseStreaming, StreamingOptions, cancellationToken);

        chatHistory.Add(new AssistantChatMessage(response));
        await SaveHistoryAsync(turnContext, chatHistory, cancellationToken);
    }

    private async Task<List<ChatMessage>> GetHistoryAsync(ITurnContext turnContext, CancellationToken cancellationToken)
    {
        var chat = (await storage.ReadAsync([turnContext.Activity.From.AadObjectId ?? turnContext.Activity.From.Id], cancellationToken)).FirstOrDefault();

        return chat.Value is not List<(string Role, string Message)> chatList
            ? []
            : chatList.Select(c => c.Role == "user"
                ? (ChatMessage)new UserChatMessage(c.Message)
                : new AssistantChatMessage(c.Message))
            .TakeLast(10)
            .ToList();
    }

    private async Task SaveHistoryAsync(ITurnContext turnContext, List<ChatMessage> chatHistory, CancellationToken cancellationToken)
    {
        var chatList = chatHistory.Select(c => c is UserChatMessage userMessage
                ? (Role: "user", Message: userMessage.Content.First().Text)
                : (Role: "assistant", Message: (c as AssistantChatMessage)?.Content.First().Text))
            .ToList();

        await storage.WriteAsync(new Dictionary<string, object>()
        {
            { turnContext.Activity.From.AadObjectId ?? turnContext.Activity.From.Id, chatList }
        }, cancellationToken);
    }
}
