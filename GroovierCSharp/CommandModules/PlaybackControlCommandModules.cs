using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;
using GroovierCSharp.Controllers;
using LyricsScraperNET;
using LyricsScraperNET.Models.Requests;

namespace GroovierCSharp.CommandModules;

public partial class PlaybackControlCommandModules : ApplicationCommandModule
{
    [SlashCommand("Volume", "Sets the volume of the player")]
    public static async Task Volume(InteractionContext ctx, [Option("volume", "The volume to set")] long volume)
    {
        await ControllerCommandModules.ConnectionSetup(ctx);
        if (volume < 0 || volume > 100)
        {
            await ctx.CreateResponseAsync("Volume must be between 0 and 100.");
            return;
        }

        await LavaLinkController.Connection.SetVolumeAsync((int)volume);
        var embed = ControllerCommandModules.EmbedCreator("Volume", $"Volume set to {volume}");
        await ctx.CreateResponseAsync(embed);
    }

    [SlashCommand("Seek", "Seeks to a position in the current track")]
    public static async Task Seek(InteractionContext ctx,
        [Option("position", "The position to seek to")]
        TimeSpan? position)
    {
        await ControllerCommandModules.ConnectionSetup(ctx);
        if (position < TimeSpan.Zero || position > LavaLinkController.Connection.CurrentState.CurrentTrack.Length)
        {
            var embed = ControllerCommandModules.EmbedCreator("Seek", "Invalid position.");
            await ctx.CreateResponseAsync(embed);
            return;
        }

        if (position != null)
        {
            await LavaLinkController.Connection.SeekAsync(position.Value);
            var embed = ControllerCommandModules.EmbedCreator("Seek", $"Seeked to {position}");
            await ctx.CreateResponseAsync(embed);
            return;
        }

        var seekEmbed = ControllerCommandModules.EmbedCreator("Seek", "Invalid position.");
        await ctx.CreateResponseAsync(seekEmbed);
    }

    [SlashCommand("Loop", "Loops the current track")]
    public static async Task Loop(InteractionContext ctx)
    {
        await ControllerCommandModules.ConnectionSetup(ctx);
        LavaLinkController.Loop = !LavaLinkController.Loop;
        await ctx.CreateResponseAsync($"Looping set to {LavaLinkController.Loop}");
        var embed = ControllerCommandModules.EmbedCreator("Loop", $"Looping set to {LavaLinkController.Loop}");
        await ctx.CreateResponseAsync(embed);
    }

    [SlashCommand("Shuffle", "Shuffles the queue")]
    public static async Task Shuffle(InteractionContext ctx)
    {
        await ControllerCommandModules.ConnectionSetup(ctx);
        GuildQueueManager.TryGetQueue(ctx.Guild.Id, out var queue);
        var queueArray = queue.ToArray();
        var history = LavaLinkController.History.ToArray();
        var shuffled = queueArray.Concat(history).ToArray();
        var rng = new Random();
        for (var i = shuffled.Length - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        queue.Clear();
        foreach (var item in shuffled) GuildQueueManager.AddTrackToQueue(ctx.Guild.Id, item);

        var embed = ControllerCommandModules.EmbedCreator("Queue", "Queue has been shuffled.");
        await ctx.CreateResponseAsync(embed);
    }

    [SlashCommand("Rewind", "Goes back to the previous track")]
    public static async Task Rewind(InteractionContext ctx)
    {
        await ControllerCommandModules.ConnectionSetup(ctx);
        if (LavaLinkController.History.Count > 0)
        {
            var previousTrack = LavaLinkController.History.Last();
            var currentTrack = LavaLinkController.Connection.CurrentState.CurrentTrack;
            ConcurrentQueue<LavalinkTrack> queueCopy = new();
            queueCopy.Enqueue(currentTrack);
            GuildQueueManager.TryGetQueue(ctx.Guild.Id, out var queue);
            foreach (var tracks in queue) queueCopy.Enqueue(tracks);

            queue = queueCopy;
            await LavaLinkController.Connection.PlayAsync(previousTrack);
            var embed = ControllerCommandModules.EmbedCreator("Rewinding",
                LavaLinkController.Connection.CurrentState.CurrentTrack.Title);
            await ctx.CreateResponseAsync(embed);
            LavaLinkController.History =
                new ConcurrentQueue<LavalinkTrack>(
                    LavaLinkController.History.Take(LavaLinkController.History.Count - 1));
            GC.Collect();
        }
        else
        {
            var embed = ControllerCommandModules.EmbedCreator("Rewinding", "No previous track.");
            await ctx.CreateResponseAsync(embed);
        }
    }

    [SlashCommand("Lyrics", "Displays the lyrics of the current track")]
    public static async Task Lyrics(InteractionContext ctx)
    {
        await ControllerCommandModules.ConnectionSetup(ctx);
        await ctx.CreateResponseAsync("Searching for lyrics...");
        var lyricsScraperClient = new LyricsScraperClient().WithAllProviders();
        var artist = LavaLinkController.Connection.CurrentState.CurrentTrack.Author;
        var track = LavaLinkController.Connection.CurrentState.CurrentTrack.Title;
        var cleanTrack = CleanTrackTitle(track, artist);
        var searchRequest = new ArtistAndSongSearchRequest(artist, cleanTrack);
        var searchResult = await lyricsScraperClient.SearchLyricAsync(searchRequest);
        if (!searchResult.IsEmpty())
        {
            DiscordEmbedBuilder embed = new()
            {
                Title = LavaLinkController.Connection.CurrentState.CurrentTrack.Title,
                Description = searchResult.LyricText,
                Color = new DiscordColor(0xb16ad4)
            };
            await ctx.DeleteResponseAsync();
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed));
            return;
        }

        await ctx.DeleteResponseAsync();
        await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Lyrics not found."));
    }

    private static string CleanTrackTitle(string title, string artist = "")
    {
        // List of words to remove from the title
        var wordsToRemove = new[]
        {
            "official", "video", "lyrics", "audio", "ft.", "feat.", "remix", "version", "lyric", "music", "hd", "hq",
            "full", "song", "original", "visualizer", "visualiser,1080p", "-", "4k", artist, "vevo", "vevoofficial"
        };

        title = wordsToRemove.Aggregate(title,
            (current, word) => current.Replace(word, "", StringComparison.OrdinalIgnoreCase));
        title = MyRegex().Replace(title, "");
        return title.Trim();
    }

    [GeneratedRegex(@"[\[\](){}]")]
    private static partial Regex MyRegex();
}