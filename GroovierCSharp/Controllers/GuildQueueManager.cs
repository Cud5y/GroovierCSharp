using System.Collections.Concurrent;
using DSharpPlus.Lavalink;

namespace GroovierCSharp.Controllers;

public readonly record struct GuildQueueManager
{
    private static readonly ConcurrentDictionary<ulong, ConcurrentQueue<LavalinkTrack>> GuildQueues = new();

    public static void AddTrackToQueue(ulong guildId, LavalinkTrack track)
    {
        var queue = GuildQueues.GetOrAdd(guildId, new ConcurrentQueue<LavalinkTrack>());
        queue.Enqueue(track);
    }

    public static bool TryGetQueue(ulong guildId, out ConcurrentQueue<LavalinkTrack> queue)
    {
#pragma warning disable CS8601 // Possible null reference assignment.
        return GuildQueues.TryGetValue(guildId, out queue);
#pragma warning restore CS8601 // Possible null reference assignment.
    }

    public static bool TryDequeueTrack(ulong guildId, out LavalinkTrack track)
    {
        if (GuildQueues.TryGetValue(guildId, out var queue))
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            return queue.TryDequeue(out track);
#pragma warning restore CS8601 // Possible null reference assignment.
        }

        track = null!;
        return false;
    }
}