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
        return GuildQueues.TryGetValue(guildId, out queue!);
    }

    public static bool TryDequeueTrack(ulong guildId, out LavalinkTrack track)
    {
        if (GuildQueues.TryGetValue(guildId, out var queue))
        {
            return queue.TryDequeue(out track!);
        }

        track = null!;
        return false;
    }

    public static void RemoveQueue(ulong guildId)
    {
        GuildQueues.TryRemove(guildId, out _);
    }
}