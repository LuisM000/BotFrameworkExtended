using Microsoft.Bot.Schema;

namespace BotFrameworkExtended.Streaming.Internals;

/// <summary>
/// Represents the state of the message processing within a streaming context, including details about the message and its position in a sequence.
/// </summary>
/// <param name="Message">The content of the message being processed.</param>
/// <param name="IsFirstMessage">Indicates if this is the first message in a sequence of messages.</param>
/// <param name="IsLastMessage">Indicates if this is the last message in a sequence of messages.</param>
/// <param name="CurrentIteration">The current iteration number in the processing sequence.</param>
/// <param name="ActivitiesSent">A collection of activities that have been sent as part of processing this message.</param>
internal record MessageProcessorState(
    string Message,
    bool IsFirstMessage,
    bool IsLastMessage,
    int CurrentIteration,
    IEnumerable<Activity> ActivitiesSent)
{
    /// <summary>
    /// Determines if the message being processed is the only message in the sequence.
    /// </summary>
    public bool IsSingleMessage => IsFirstMessage && IsLastMessage;
}
