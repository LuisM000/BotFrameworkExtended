namespace BotFrameworkExtended.Streaming.Options;

/// <summary>
/// Options for configuring the behavior of streaming messages.
/// </summary>
public record StreamingMessageOptions
{
    /// <summary>
    /// The delay before processing each message. This can help in managing the rate at which messages are processed.
    /// </summary>
    public TimeSpan MessageProcessingDelay { get; init; } = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// The delay before starting the processing of messages. This can be useful for initial setup.
    /// </summary>
    public TimeSpan InitialProcessingDelay { get; init; } = TimeSpan.FromMilliseconds(200);

    /// <summary>
    /// The delay to apply when no messages are available to process. This can help in reducing resource usage when the message queue is empty.
    /// </summary>
    public TimeSpan NoMessageDelay { get; init; } = TimeSpan.FromMilliseconds(50);
}
