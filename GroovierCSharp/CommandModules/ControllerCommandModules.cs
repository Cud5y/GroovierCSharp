using System.Collections.Concurrent;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.SlashCommands;

namespace GroovierCSharp.CommandModules;

public class ControllerCommandModules : ApplicationCommandModule
{
    private static LavalinkGuildConnection _connection = null!;
    private static bool _skip = false;
    public static ConcurrentQueue<LavalinkTrack> Queue { get; } = new();
    public static ConcurrentQueue<LavalinkTrack> History { get; } = new();

    [SlashCommand("play", "Plays a song")]
    public static async Task Play(InteractionContext ctx, [Option("query", "The song to play")] string query)
    {
        var vnext = ctx.Client.GetLavalink();
        if (vnext?.ConnectedNodes.Values.FirstOrDefault() is not LavalinkNodeConnection node)
        {
            await ctx.CreateResponseAsync("Lavalink is not connected or no connection nodes found.");
            return;
        }

        _connection = node.GetGuildConnection(ctx.Guild);
        if (_connection == null)
        {
            var channel = ctx.Member.VoiceState.Channel;
            await node.ConnectAsync(channel);
            _connection = node.GetGuildConnection(ctx.Guild);
        }

        _connection.PlaybackFinished += OnPlaybackFinished;

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
            if (_connection.CurrentState.CurrentTrack is not null)
            {
                Queue.Enqueue(track);
                await ctx.CreateResponseAsync($"Added {track.Title} to the queue.");
            }
            else
            {
                await _connection.PlayAsync(track);
                await ctx.CreateResponseAsync($"Playing {track.Title}");
            }
        }
        catch (Exception ex)
        {
            await ctx.CreateResponseAsync($"An error occurred: {ex.Message}");
        }
    }

    private static async Task OnPlaybackFinished(LavalinkGuildConnection sender, TrackFinishEventArgs e)
    {
        if (Queue.TryDequeue(out var nextTrack))
        {
            await sender.PlayAsync(nextTrack);
        }
    }


    [SlashCommand("Pause", "Pauses the current song")]
    public static async Task Pause(InteractionContext ctx)
    {
        var vnext = ctx.Client.GetLavalink();
        if (vnext is null)
        {
            await ctx.CreateResponseAsync("Lavalink is not connected.");
            return;
        }

        var node = vnext.ConnectedNodes.Values.First();
        if (node is null)
        {
            await ctx.CreateResponseAsync("No connection nodes found.");
            return;
        }

        if (_connection.CurrentState.CurrentTrack is null)
        {
            await ctx.CreateResponseAsync("Nothing is currently playing.");
            return;
        }

        await _connection.PauseAsync();
        await ctx.CreateResponseAsync("Paused");
    }

    [SlashCommand("Resume", "Resumes the current song")]
    public static async Task Resume(InteractionContext ctx)
    {
        var vnext = ctx.Client.GetLavalink();
        if (vnext is null)
        {
            await ctx.CreateResponseAsync("Lavalink is not connected.");
            return;
        }

        var node = vnext.ConnectedNodes.Values.First();
        if (node is null)
        {
            await ctx.CreateResponseAsync("No connection nodes found.");
            return;
        }

        var connection = node.GetGuildConnection(ctx.Guild);
        if (connection is null)
        {
            await ctx.CreateResponseAsync("No active connection found.");
            return;
        }

        if (connection.CurrentState.CurrentTrack is null)
        {
            await ctx.CreateResponseAsync("Nothing is currently playing.");
            return;
        }

        await connection.ResumeAsync();
        await ctx.CreateResponseAsync("Resumed");
    }

    [SlashCommand("Stop", "Stops the current song")]
    public static async Task Stop(InteractionContext ctx)
    {
        var vnext = ctx.Client.GetLavalink();
        if (vnext is null)
        {
            await ctx.CreateResponseAsync("Lavalink is not connected.");
            return;
        }

        var node = vnext.ConnectedNodes.Values.First();
        if (node is null)
        {
            await ctx.CreateResponseAsync("No connection nodes found.");
            return;
        }

        if (_connection.CurrentState.CurrentTrack is null)
        {
            await ctx.CreateResponseAsync("Nothing Qis currently playing.");
            return;
        }

        Queue.Clear();
        await _connection.StopAsync();
        await ctx.CreateResponseAsync("Stopped");
    }

    [SlashCommand("Skip", "Skips the current song")]
    public static async Task Skip(InteractionContext ctx)
    {
        var vnext = ctx.Client.GetLavalink();
        if (vnext is null)
        {
            await ctx.CreateResponseAsync("Lavalink is not connected.");
            return;
        }

        var node = vnext.ConnectedNodes.Values.First();
        if (node is null)
        {
            await ctx.CreateResponseAsync("No connection nodes found.");
            return;
        }

        if (_connection.CurrentState.CurrentTrack is null)
        {
            await ctx.CreateResponseAsync("Nothing is currently playing.");
            return;
        }

        await ctx.CreateResponseAsync($"Skipping {_connection.CurrentState.CurrentTrack.Title}");
        await _connection.StopAsync();
    }
}