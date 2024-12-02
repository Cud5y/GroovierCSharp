using System.Text;
using DSharpPlus.SlashCommands;
using GroovierCSharp.Controllers;

namespace GroovierCSharp.CommandModules;

public class QueueControlCommandModules : ApplicationCommandModule
{
    [SlashCommand("Queue", "Shows the current queue")]
    public static async Task Queue(InteractionContext ctx)
    {
        GuildQueueManager.TryGetQueue(ctx.Guild.Id, out var queue);
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
        var history = LavaLinkController.History;
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
        GuildQueueManager.TryGetQueue(ctx.Guild.Id, out var queue);
        queue.Clear();
        var embed = ControllerCommandModules.EmbedCreator("Queue", "The queue has been cleared.");
        await ctx.CreateResponseAsync(embed);
    }

    [SlashCommand("Remove", "Removes a song from the queue")]
    public static async Task Remove(InteractionContext ctx,
        [Option("index", "The index of the song to remove")]
        long index)
    {
        GuildQueueManager.TryGetQueue(ctx.Guild.Id, out var queue);
        index -= 1;
        if (index < 0 || index >= queue.Count)
        {
            var embed = ControllerCommandModules.EmbedCreator("Queue", "Invalid index.");
            await ctx.CreateResponseAsync(embed);
            return;
        }

        var queueList = queue.ToList();
        var track = queueList[(int)index];
        queueList.RemoveAt((int)index);
        queue.Clear();
        foreach (var item in queueList) GuildQueueManager.AddTrackToQueue(ctx.Guild.Id, item);

        var removeEmbed = ControllerCommandModules.EmbedCreator("Queue", $"Removed {track.Title} from the queue.");
        await ctx.CreateResponseAsync(removeEmbed);
        GC.Collect();
    }

    [SlashCommand("NowPlaying", "Shows the currently playing song")]
    public static async Task NowPlaying(InteractionContext ctx)
    {
        var track = LavaLinkController.Connection.CurrentState.CurrentTrack;
        var trackPosition = LavaLinkController.Connection.CurrentState.PlaybackPosition.ToString(@"hh\:mm\:ss");
        if (track is null)
        {
            var embed = ControllerCommandModules.EmbedCreator("Now Playing", "Nothing is currently playing.");
            await ctx.CreateResponseAsync(embed);
            return;
        }

        var nowPlayingEmbed = ControllerCommandModules.EmbedCreator("Now Playing",
            $"Now playing: {track.Title}\n{trackPosition} : {track.Length.ToString(@"hh\:mm\:ss")}");
        await ctx.CreateResponseAsync(nowPlayingEmbed);
    }
}