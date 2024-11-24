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
        if (index < 0 || index >= ControllerCommandModules.Queue.Count)
        {
            await ctx.CreateResponseAsync("Invalid index.");
            return;
        }

        var track = ControllerCommandModules.Queue.ElementAt((int)index);
        ControllerCommandModules.Queue =
            new ConcurrentQueue<LavalinkTrack>(ControllerCommandModules.Queue.Where(t => t != track));
        await ctx.CreateResponseAsync($"Removed {track.Title} from the queue.");
    }

    [SlashCommand("NowPlaying", "Shows the currently playing song")]
    public static async Task NowPlaying(InteractionContext ctx)
    {
        var track = ControllerCommandModules.Connection.CurrentState.CurrentTrack;
        var trackPosition = ControllerCommandModules.Connection.CurrentState.PlaybackPosition.ToString(@"hh\:mm\:ss");
        if (track is null)
        {
            await ctx.CreateResponseAsync("Nothing is currently playing.");
            return;
        }

        await ctx.CreateResponseAsync($"Now playing: {track.Title}\n{trackPosition} : {track.Length}");
    }
}