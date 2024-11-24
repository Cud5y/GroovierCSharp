using System.Collections.Concurrent;
using System.Text;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;

namespace GroovierCSharp.CommandModules;

public class QueueControlCommandModules : ApplicationCommandModule
{
    [SlashCommand("Queue", "Shows the current queue")]
    public static async Task Queue(InteractionContext ctx)
    {
        var queue = ControllerCommandModules.Queue;
        if (queue.Count == 0)
        {
            await ctx.CreateResponseAsync("The queue is empty.");
            return;
        }

        var sb = new StringBuilder();
        foreach (var track in queue) sb.AppendLine(track.Title);

        await ctx.CreateResponseAsync(sb.ToString());
    }

    [SlashCommand("History", "Shows the history of played songs")]
    public static async Task History(InteractionContext ctx)
    {
        var history = ControllerCommandModules.History;
        if (history.Count == 0)
        {
            await ctx.CreateResponseAsync("The history is empty.");
            return;
        }

        var sb = new StringBuilder();
        foreach (var track in history) sb.AppendLine(track.Title);

        await ctx.CreateResponseAsync(sb.ToString());
    }

    [SlashCommand("Clear", "Clears the queue")]
    public static async Task Clear(InteractionContext ctx)
    {
        ControllerCommandModules.Queue.Clear();
        await ctx.CreateResponseAsync("The queue has been cleared.");
    }

    [SlashCommand("Remove", "Removes a song from the queue")]
    public static async Task Remove(InteractionContext ctx,
        [Option("index", "The index of the song to remove")]
        long index)
    {
        var queue = ControllerCommandModules.Queue;
        if (index < 0 || index >= queue.Count)
        {
            await ctx.CreateResponseAsync("Invalid index.");
            return;
        }

        var track = queue.ElementAt((int)index);
        queue = new ConcurrentQueue<LavalinkTrack>(queue.Where(t => t != track));
        await ctx.CreateResponseAsync($"Removed {track.Title} from the queue.");
    }
}