using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;
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

        await ControllerCommandModules.Connection.SetVolumeAsync((int)volume);
        await ctx.CreateResponseAsync($"Volume set to {volume}");
    }

    [SlashCommand("Seek", "Seeks to a position in the current track")]
    public static async Task Seek(InteractionContext ctx,
        [Option("position", "The position to seek to")]
        TimeSpan? position)
    {
        await ControllerCommandModules.ConnectionSetup(ctx);
        if (position < TimeSpan.Zero || position > ControllerCommandModules.Connection.CurrentState.CurrentTrack.Length)
        {
            await ctx.CreateResponseAsync("Invalid position.");
            return;
        }

        if (position != null)
        {
            await ControllerCommandModules.Connection.SeekAsync(position.Value);
            await ctx.CreateResponseAsync($"Seeked to {position}S");
        }

        await ctx.CreateResponseAsync("Invalid position.");
    }

    [SlashCommand("Loop", "Loops the current track")]
    public static async Task Loop(InteractionContext ctx)
    {
        await ControllerCommandModules.ConnectionSetup(ctx);
        ControllerCommandModules.Loop = !ControllerCommandModules.Loop;
        await ctx.CreateResponseAsync($"Looping set to {ControllerCommandModules.Loop}");
    }

    [SlashCommand("Shuffle", "Shuffles the queue")]
    public static async Task Shuffle(InteractionContext ctx)
    {
        await ControllerCommandModules.ConnectionSetup(ctx);
        var queue = ControllerCommandModules.Queue.ToArray();
        var history = ControllerCommandModules.History.ToArray();
        var shuffled = queue.Concat(history).ToArray();
        var rng = new Random();
        for (var i = shuffled.Length - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        ControllerCommandModules.Queue = new ConcurrentQueue<LavalinkTrack>(shuffled);
        await ctx.CreateResponseAsync("Queue shuffled.");
    }

    [SlashCommand("Rewind", "Goes back to the previous track")]
    public static async Task Rewind(InteractionContext ctx)
    {
        await ControllerCommandModules.ConnectionSetup(ctx);
        if (ControllerCommandModules.History.Count > 0)
        {
            var previousTrack = ControllerCommandModules.History.Last();
            var currentTrack = ControllerCommandModules.Connection.CurrentState.CurrentTrack;
            ConcurrentQueue<LavalinkTrack> queueCopy = new();
            queueCopy.Enqueue(currentTrack);
            foreach (var tracks in ControllerCommandModules.Queue) queueCopy.Enqueue(tracks);

            ControllerCommandModules.Queue = queueCopy;
            await ControllerCommandModules.Connection.PlayAsync(previousTrack);
            await ctx.CreateResponseAsync($"Rewinding to {previousTrack.Title}");
            ControllerCommandModules.History =
                new ConcurrentQueue<LavalinkTrack>(
                    ControllerCommandModules.History.Take(ControllerCommandModules.History.Count - 1));
            GC.Collect();
        }
        else
        {
            await ctx.CreateResponseAsync("No previous track.");
        }
    }

    [SlashCommand("Lyrics", "Displays the lyrics of the current track")]
    public static async Task Lyrics(InteractionContext ctx)
    {
        await ControllerCommandModules.ConnectionSetup(ctx);
        await ctx.CreateResponseAsync("Searching for lyrics...");
        var lyricsScraperClient = new LyricsScraperClient().WithAllProviders();
        var artist = ControllerCommandModules.Connection.CurrentState.CurrentTrack.Author;
        var track = ControllerCommandModules.Connection.CurrentState.CurrentTrack.Title;
        var cleanTrack = CleanTrackTitle(track, artist);
        var searchRequest = new ArtistAndSongSearchRequest(artist, cleanTrack);
        var searchResult = await lyricsScraperClient.SearchLyricAsync(searchRequest);
        if (!searchResult.IsEmpty())
        {
            DiscordEmbedBuilder embed = new()
            {
                Title = ControllerCommandModules.Connection.CurrentState.CurrentTrack.Title,
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
            "full", "song", "original", "visualizer", "visualiser,1080p", "-", "4k", artist
        };

        title = wordsToRemove.Aggregate(title,
            (current, word) => current.Replace(word, "", StringComparison.OrdinalIgnoreCase));
        title = MyRegex().Replace(title, "");
        return title.Trim();
    }

    [GeneratedRegex(@"[\[\](){}]")]
    private static partial Regex MyRegex();
}