using BotFrameworkExtended.Streaming.Internals;
using BotFrameworkExtended.Streaming.Options;

using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;

namespace BotFrameworkExtended.Streaming.Extensions;

// https://learn.microsoft.com/en-us/microsoftteams/platform/bots/how-to/rate-limit

/// <summary>
/// Extensions for handling streaming messages in a <see cref="ITurnContext"/>.
/// </summary>
public static class ITurnContextExtensions
{
    /// <summary>
    /// Sends a streaming message.
    /// </summary>
    /// <param name="turnContext">The <see cref="ITurnContext"/> for the current conversation.</param>
    /// <param name="streamingMessage">The <see cref="IAsyncEnumerable{T}"/> of strings representing the streaming message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response from processing the streaming message.</returns>
    /// <exception cref="NotSupportedException">Thrown if streaming is not supported for the current channel.</exception>
    public static Task<string> SendStreamingMessageAsync(this ITurnContext turnContext, IAsyncEnumerable<string> streamingMessage, CancellationToken cancellationToken)
    {
        return SendStreamingMessageAsync(turnContext, streamingMessage, new StreamingMessageOptions(), cancellationToken);
    }

    /// <summary>
    /// Sends a streaming message.
    /// </summary>
    /// <param name="turnContext">The <see cref="ITurnContext"/> for the current conversation.</param>
    /// <param name="streamingMessage">The <see cref="IAsyncEnumerable{T}"/> of strings representing the streaming message.</param>
    /// <param name="options">The <see cref="StreamingMessageOptions"/> to configure the streaming message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">Thrown if streaming is not supported for the current channel.</exception>
    public static Task<string> SendStreamingMessageAsync(this ITurnContext turnContext, IAsyncEnumerable<string> streamingMessage, StreamingMessageOptions options, CancellationToken cancellationToken)
    {

        var messageProcessor = turnContext.Activity.ChannelId switch
        {
            Channels.Webchat => new StreamingMessageProcessor(SendWebchatActivityAsync),
            Channels.Msteams => new StreamingMessageProcessor(SendMsTeamsActivityAsync),
            _ => throw new NotSupportedException($"Streaming is not supported for channel {turnContext.Activity.ChannelId}")
        };

        return messageProcessor.ProcessMessagesAsync(turnContext, streamingMessage, options, cancellationToken);
    }

    private static async Task<Activity> SendMsTeamsActivityAsync(ITurnContext turnContext, MessageProcessorState state, CancellationToken cancellationToken)
    {
        Activity activity;

        if (state.IsFirstMessage)
        {
            // If is first message, send it directly
            activity = MessageFactory.Text(state.Message, state.Message);

            await turnContext.SendActivityAsync(activity, cancellationToken);
        }
        else
        {
            // If is not first message, update the last message sent
            activity = state.ActivitiesSent.Last();
            activity.Text = state.Message;

            await turnContext.UpdateActivityAsync(activity, cancellationToken);
        }

        return activity;
    }

    private static async Task<Activity> SendWebchatActivityAsync(ITurnContext turnContext, MessageProcessorState state, CancellationToken cancellationToken)
    {
        Activity activity;

        if (state.IsSingleMessage)
        {
            // Single message case
            activity = MessageFactory.Text(state.Message, state.Message);
        }
        else
        {
            // Streaming message case.
            // Manages first message, intermediate messages and last message
            activity = new Activity
            {
                Text = state.Message,
                Speak = state.IsLastMessage ? state.Message : null,
                Type = state.IsFirstMessage ? ActivityTypes.Message : ActivityTypes.Typing,
                ChannelData = new
                {
                    streamType = state.IsLastMessage ? "final" : "streaming",
                    streamSequence = state.CurrentIteration,
                    streamId = state.ActivitiesSent.FirstOrDefault()?.Id
                }
            };
        }

        await turnContext.SendActivityAsync(activity, cancellationToken);
        return activity;
    }
}
