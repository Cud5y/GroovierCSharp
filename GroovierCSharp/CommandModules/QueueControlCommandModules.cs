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
            var embed = ControllerCommandModules.EmbedCreator("Queue", "The queue is empty.");
            await ctx.CreateResponseAsync(embed);
            return;
        }

        var sb = new StringBuilder();
        for (var i = 0; i < queue.Count; i++) sb.AppendLine($"{i + 1}. {queue.ElementAt(i).Title}");

        var queueEmbed = ControllerCommandModules.EmbedCreator("Queue", sb.ToString());
        await ctx.CreateResponseAsync(queueEmbed);
    }

    [SlashCommand("History", "Shows the history of played songs")]
    public static async Task History(InteractionContext ctx)
    {
        var history = ControllerCommandModules.History;
        if (history.Count == 0)
        {
            var embed = ControllerCommandModules.EmbedCreator("History", "The history is empty.");
            await ctx.CreateResponseAsync(embed);
            return;
        }

        var sb = new StringBuilder();
        for (var i = 0; i < history.Count; i++) sb.AppendLine($"{i + 1}. {history.ElementAt(i).Title}");

        var historyEmbed = ControllerCommandModules.EmbedCreator("History", sb.ToString());
        await ctx.CreateResponseAsync(historyEmbed);
    }

    [SlashCommand("Clear", "Clears the queue")]
    public static async Task Clear(InteractionContext ctx)
    {
        ControllerCommandModules.Queue.Clear();
        var embed = ControllerCommandModules.EmbedCreator("Queue", "The queue has been cleared.");
        await ctx.CreateResponseAsync(embed);
    }

    [SlashCommand("Remove", "Removes a song from the queue")]
    public static async Task Remove(InteractionContext ctx,
        [Option("index", "The index of the song to remove")]
        long index)
    {
        index++;
        if (index < 1 || index >= ControllerCommandModules.Queue.Count)
        {
            var embed = ControllerCommandModules.EmbedCreator("Queue", "Invalid index.");
            await ctx.CreateResponseAsync(embed);
            return;
        }

        var track = ControllerCommandModules.Queue.ElementAt((int)index);
        ControllerCommandModules.Queue =
            new ConcurrentQueue<LavalinkTrack>(ControllerCommandModules.Queue.Where(t => t != track));
        var removeEmbed = ControllerCommandModules.EmbedCreator("Queue", $"Removed {track.Title} from the queue.");
        await ctx.CreateResponseAsync(removeEmbed);
    }

    [SlashCommand("NowPlaying", "Shows the currently playing song")]
    public static async Task NowPlaying(InteractionContext ctx)
    {
        var track = ControllerCommandModules.Connection.CurrentState.CurrentTrack;
        var trackPosition = ControllerCommandModules.Connection.CurrentState.PlaybackPosition.ToString(@"hh\:mm\:ss");
        if (track is null)
        {
            var embed = ControllerCommandModules.EmbedCreator("Now Playing", "Nothing is currently playing.");
            await ctx.CreateResponseAsync(embed);
            return;
        }

        var nowPlayingEmbed = ControllerCommandModules.EmbedCreator("Now Playing",
            $"Now playing: {track.Title}\n{trackPosition} : {track.Length}");
        await ctx.CreateResponseAsync(nowPlayingEmbed);
    }
}