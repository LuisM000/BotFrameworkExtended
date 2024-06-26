using Microsoft.Bot.Builder;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using BotFrameworkExtended.Streaming.Extensions;

using Activity = Microsoft.Bot.Schema.Activity;
using BotFrameworkExtended.Streaming.Options;

namespace BotFrameworkExtended.Streaming.Internals;

/// <summary>
/// Handles processing of incoming streaming messages asynchronously.
/// </summary>
/// <param name="sendActivity">Function delegate to send activity.</param>
internal class StreamingMessageProcessor(Func<ITurnContext, MessageProcessorState, CancellationToken, Task<Activity>> sendActivity)
{
    private readonly StringBuilder aggregateMessage = new();
    private BlockingCollection<string> messages = [];

    /// <summary>
    /// Processes incoming streaming messages asynchronously. Manages the accumulation,
    /// processing, and dispatching of activities based on the received messages.
    /// </summary>
    /// <param name="turnContext">The <see cref="ITurnContext"/> for the current conversation.</param>
    /// <param name="streamingMessage">The <see cref="IAsyncEnumerable{T}"/> of strings representing the streaming message.</param>
    /// <param name="options">The <see cref="StreamingMessageOptions"/> to configure the streaming message.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the aggregated messages as a single string.</returns>
    public async Task<string> ProcessMessagesAsync(ITurnContext turnContext, IAsyncEnumerable<string> streamingMessage, StreamingMessageOptions options, CancellationToken cancellationToken)
    {
        messages = [];
        aggregateMessage.Clear();

        await Task.WhenAll(
            ProcessStreamingMessageAsync(streamingMessage, cancellationToken),
            ProcessMessagesLoopAsync(turnContext, options, cancellationToken));

        return aggregateMessage.ToString();
    }

    private async Task ProcessStreamingMessageAsync(IAsyncEnumerable<string> streamingMessage, CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var message in streamingMessage.WithCancellation(cancellationToken))
            {
                messages.Add(message, cancellationToken);
            }

            messages.CompleteAdding();
        }
        finally
        {
            messages.CompleteAdding();
        }
    }

    private async Task ProcessMessagesLoopAsync(ITurnContext turnContext, StreamingMessageOptions options, CancellationToken cancellationToken)
    {
        var stopwatch = new Stopwatch();
        var isFirstMessage = true;
        var activities = new List<Activity>();
        var currentIteration = 1;

        // Process messages until the blocking collection is completed.
        while (!messages.IsCompleted)
        {
            stopwatch.Restart();

            // Initial delay before processing the first message.
            // This is useful for scenarios where the first message is delayed.
            if (isFirstMessage && options.InitialProcessingDelay > TimeSpan.Zero)
            {
                await Task.Delay(options.InitialProcessingDelay, cancellationToken);
            }

            // Take all available messages from the blocking collection.
            var newMessages = messages.TakeCurrent(cancellationToken);
            if (newMessages.Range == 0 && options.NoMessageDelay > TimeSpan.Zero)
            {
                await Task.Delay(options.NoMessageDelay, cancellationToken);
                continue;
            }

            // Append all new messages to the aggregate message buffer.
            aggregateMessage.Append(string.Concat(newMessages.Data));
            if (aggregateMessage.Length <= 0 && options.NoMessageDelay > TimeSpan.Zero)
            {
                await Task.Delay(options.NoMessageDelay, cancellationToken);
                continue;
            }

            // Callback to send the activity with the current state.
            var activity = await sendActivity(turnContext, new MessageProcessorState(aggregateMessage.ToString(), isFirstMessage, messages.IsCompleted, currentIteration, activities.Distinct().ToList()), cancellationToken);

            activities.Add(activity);
            currentIteration++;
            isFirstMessage = false;

            stopwatch.Stop();

            // Calculate remaining time before the next processing cycle.
            var remainingTime = options.MessageProcessingDelay - stopwatch.Elapsed;
            if (remainingTime > TimeSpan.Zero)
            {
                await Task.Delay(remainingTime, cancellationToken);
            }
        }
    }
}
