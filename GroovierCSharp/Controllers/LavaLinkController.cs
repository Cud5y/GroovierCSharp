using System.Collections.Concurrent;
using DSharpPlus.Lavalink;

namespace GroovierCSharp.Controllers;

public readonly record struct LavaLinkController
{
    public static LavalinkExtension Vnext { get; set; } = null!;
    public static LavalinkNodeConnection Node { get; set; } = null!;
    public static bool Loop { get; set; }
    public static LavalinkGuildConnection Connection { get; set; } = null!;
    public static ConcurrentQueue<LavalinkTrack> History { get; set; } = new();
}