using System.Collections.Concurrent;
using DSharpPlus.Lavalink;

namespace GroovierCSharp.Controllers;

public readonly record struct HistoryQueueManager
{
    private static readonly ConcurrentDictionary<ulong, ConcurrentQueue<LavalinkTrack>> GuildHistory = new();

    public static void AddTrackToHistory(ulong guildId, LavalinkTrack track)
    {
        var queue = GuildHistory.GetOrAdd(guildId, new ConcurrentQueue<LavalinkTrack>());
        queue.Enqueue(track);
    }

    public static bool TryGetHistory(ulong guildId, out ConcurrentQueue<LavalinkTrack> queue)
    {
        return GuildHistory.TryGetValue(guildId, out queue!);
    }

    public static bool TryDequeueTrack(ulong guildId, out LavalinkTrack track)
    {
        if (GuildHistory.TryGetValue(guildId, out var queue)) return queue.TryDequeue(out track!);

        track = null!;
        return false;
    }

    public static void RemoveHistory(ulong guildId)
    {
        GuildHistory.TryRemove(guildId, out _);
    }
}