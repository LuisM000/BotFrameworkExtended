using System.Collections.Concurrent;

namespace BotFrameworkExtended.Streaming.Extensions;

/// <summary>
/// Extensions for <see cref="BlockingCollection{T}"/>.
/// </summary>
internal static class BlockingCollectionExtensions
{
    /// <summary>
    /// Takes the current messages from the <see cref="BlockingCollection{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="blockingCollection">The blocking collection from which to take messages.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    /// <returns>A tuple containing the range of messages taken and the collection of messages.</returns>
    public static (int Range, ICollection<T> Data) TakeCurrent<T>(this BlockingCollection<T> blockingCollection, CancellationToken cancellationToken)
    {
        var currentRange = blockingCollection.Count;
        var currentMessages = new List<T>();

        for (var i = 0; i < currentRange; i++)
        {
            currentMessages.Add(blockingCollection.Take(cancellationToken));
        }

        return (currentRange, currentMessages);
    }
}
