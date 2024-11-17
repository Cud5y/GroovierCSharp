using System.Collections.Concurrent;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;

namespace GroovierCSharp;

public class ControllerCommandModules : ApplicationCommandModule
{
    public static ConcurrentQueue<LavalinkTrack> Queue { get; } = new();

    public static ConcurrentQueue<LavalinkTrack> History { get; } = new();

    [SlashCommand("Play", "Plays a song")]
    public static async Task Play(InteractionContext ctx, [Option("query", "The song to play")] string query)
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

        var connection = node.GetGuildConnection(ctx.Guild) ?? await node.ConnectAsync(ctx.Member.VoiceState.Channel)
            .ContinueWith(_ => node.GetGuildConnection(ctx.Guild));

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
            Queue.Enqueue(track);
            if (connection.CurrentState.CurrentTrack is null)
            {
                await PlayNext(connection, ctx);
                return;
            }

            await ctx.CreateResponseAsync($"Added {track.Title} to the queue");
        }
        catch (Exception ex)
        {
            await ctx.CreateResponseAsync($"An error occurred: {ex.Message}");
        }
    }

    private static async Task PlayNext(LavalinkGuildConnection connection, InteractionContext ctx)
    {
        if (Queue.TryDequeue(out var track))
        {
            if (connection.CurrentState.CurrentTrack is null) await ctx.CreateResponseAsync($"Playing {track.Title}");

            History.Enqueue(track);
            await connection.PlayAsync(track);
            if (!Queue.IsEmpty)
            {
                connection.PlaybackFinished += async (_, _) => await PlayNext(connection, ctx);
                return;
            }
        }
        else
        {
            await ctx.CreateResponseAsync("Queue is empty.");
        }

        if (connection.CurrentState.CurrentTrack is not null)
        {
            await ctx.CreateResponseAsync($"Stopping {connection.CurrentState.CurrentTrack.Title}");
            await connection.StopAsync();
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

        await connection.PauseAsync();
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

        await connection.StopAsync();
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

        await ctx.CreateResponseAsync($"Skipping {connection.CurrentState.CurrentTrack.Title}");
        await PlayNext(connection, ctx);
    }
}