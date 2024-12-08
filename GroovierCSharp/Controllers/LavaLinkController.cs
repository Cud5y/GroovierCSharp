using DSharpPlus.Lavalink;

namespace GroovierCSharp.Controllers;

public readonly record struct LavaLinkController
{
    public static LavalinkExtension Vnext { get; set; } = null!;
    public static LavalinkNodeConnection Node { get; set; } = null!;
    public static Dictionary<ulong, bool> Loop { get; set; } = new();
    public static Dictionary<ulong, LavalinkGuildConnection> Connection { get; set; } = new();
}