using System.Collections.Concurrent;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.SlashCommands;

namespace GroovierCSharp.CommandModules;

public class ControllerCommandModules : ApplicationCommandModule
{
    private static LavalinkExtension _vnext = null!;
    private static LavalinkNodeConnection _node = null!;
    public static bool Loop { get; set; } = false;
    public static LavalinkGuildConnection Connection { get; private set; } = null!;
    public static ConcurrentQueue<LavalinkTrack> Queue { get; set; } = new();
    public static ConcurrentQueue<LavalinkTrack> History { get; set; } = new();

    [SlashCommand("play", "Plays a song")]
    public static async Task Play(InteractionContext ctx, [Option("query", "The song to play")] string query)
    {
        _vnext = ctx.Client.GetLavalink();
        if (_vnext?.ConnectedNodes.Values.FirstOrDefault() is not LavalinkNodeConnection node)
        {
            await ctx.CreateResponseAsync("Lavalink is not connected or no connection nodes found.");
            return;
        }

        Connection = node.GetGuildConnection(ctx.Guild);
        if (Connection == null)
        {
            var channel = ctx.Member.VoiceState.Channel;
            await node.ConnectAsync(channel);
            Connection = node.GetGuildConnection(ctx.Guild);
        }

        Connection.PlaybackFinished += OnPlaybackFinished;

        try
        {
            var loadResult = await node.Rest.GetTracksAsync(query);
            switch (loadResult.LoadResultType)
            {
                case LavalinkLoadResultType.NoMatches:
                    await ctx.CreateResponseAsync("No matches found for the query.");
                    return;
                case LavalinkLoadResultType.LoadFailed:
                    await ctx.CreateResponseAsync("Failed to load tracks.");
                    return;
            }

            var track = loadResult.Tracks.First();
            if (Connection.CurrentState.CurrentTrack is not null)
            {
                Queue.Enqueue(track);
                var embed = EmbedCreator("Added to Queue", track.Title);
                await ctx.CreateResponseAsync(embed);
            }
            else
            {
                History.Enqueue(track);
                await Connection.PlayAsync(track);
                var embed = EmbedCreator("Playing", track.Title);
                await ctx.CreateResponseAsync(embed);
            }
        }
        catch (Exception ex)
        {
            await ctx.CreateResponseAsync($"An error occurred: {ex.Message}");
        }
    }

    private static async Task OnPlaybackFinished(LavalinkGuildConnection sender, TrackFinishEventArgs e)
    {
        if (Loop)
        {
            History.Enqueue(History.Last());
            await Connection.PlayAsync(History.Last());
            Connection.PlaybackFinished += OnPlaybackFinished;
            return;
        }

        if (Queue.TryDequeue(out var nextTrack))
        {
            History.Enqueue(nextTrack);
            await sender.PlayAsync(nextTrack);
        }
    }


    [SlashCommand("Pause", "Pauses the current song")]
    public static async Task Pause(InteractionContext ctx)
    {
        await ConnectionSetup(ctx);
        await Connection.PauseAsync();
        var embed = EmbedCreator("Paused", Connection.CurrentState.CurrentTrack.Title);
        await ctx.CreateResponseAsync(embed);
    }

    [SlashCommand("Resume", "Resumes the current song")]
    public static async Task Resume(InteractionContext ctx)
    {
        await ConnectionSetup(ctx);
        await Connection.ResumeAsync();
        var embed = EmbedCreator("Resumed", Connection.CurrentState.CurrentTrack.Title);
        await ctx.CreateResponseAsync(embed);
    }

    [SlashCommand("Stop", "Stops the current song")]
    public static async Task Stop(InteractionContext ctx)
    {
        await ConnectionSetup(ctx);
        Queue.Clear();
        await Connection.StopAsync();
        var embed = EmbedCreator("Stopped", Connection.CurrentState.CurrentTrack.Title);
        await ctx.CreateResponseAsync(embed);
    }

    [SlashCommand("Skip", "Skips the current song")]
    public static async Task Skip(InteractionContext ctx)
    {
        await ConnectionSetup(ctx);
        var embed = EmbedCreator("Skipping", Connection.CurrentState.CurrentTrack.Title);
        await ctx.CreateResponseAsync(embed);
        await Connection.StopAsync();
    }

    public static async Task ConnectionSetup(InteractionContext ctx)
    {
        _vnext = ctx.Client.GetLavalink();
        if (_vnext is null)
        {
            await ctx.CreateResponseAsync("Lavalink is not connected.");
            return;
        }

        _node = _vnext.ConnectedNodes.Values.First();
        if (_node is null)
        {
            await ctx.CreateResponseAsync("No connection nodes found.");
            return;
        }

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (Connection is null)
        {
            await ctx.CreateResponseAsync("No active connection found.");
            return;
        }

        if (Connection.CurrentState.CurrentTrack is null)
            await ctx.CreateResponseAsync("Nothing is currently playing.");
    }

    public static DiscordEmbed EmbedCreator(string title, string description)
    {
        return new DiscordEmbedBuilder()
            .WithTitle(title)
            .WithDescription(description)
            .WithColor(new DiscordColor(0xb16ad4));
    }
}