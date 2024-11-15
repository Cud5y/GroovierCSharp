using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;

namespace GroovierCSharp;

public class ControllerCommandModules : ApplicationCommandModule
{
    [SlashCommand("ping", "Replies with Pong!")]
    public static async Task Ping(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync("Pong!");
    }

    [SlashCommand("join", "Joins the voice channel")]
    public async Task join(InteractionContext ctx)
    {
        try
        {
            var vnext = ctx.Client.GetLavalink();
            if (vnext is null)
            {
                await ctx.CreateResponseAsync("Lavalink is not connected.");
                return;
            }

            var node = vnext.ConnectedNodes.Values.First();
            var channel = ctx.Member.VoiceState.Channel;
            await node.ConnectAsync(channel);
            await ctx.CreateResponseAsync($"Joined {channel.Name}");
        }
        catch (Exception)
        {
            await ctx.CreateResponseAsync("You need to be in a voice channel.");
        }
    }

    [SlashCommand("leave", "Leaves the voice channel")]
    public async Task leave(InteractionContext ctx)
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

        await connection.DisconnectAsync();
        await ctx.CreateResponseAsync($"Left {connection.Channel.Name}");
    }

    [SlashCommand("play", "Plays a song")]
    public async Task Play(InteractionContext ctx, [Option("query", "The song to play")] string query)
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

        var loadResult = await node.Rest.GetTracksAsync(query);
        if (loadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
        {
            await ctx.CreateResponseAsync("No matches found.");
            return;
        }

        var track = loadResult.Tracks.First();
        await connection.PlayAsync(track);
        await ctx.CreateResponseAsync($"Playing {track.Title}");
    }

    [SlashCommand("pause", "Pauses the current song")]
    public async Task Pause(InteractionContext ctx)
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

        await connection.PauseAsync();
        await ctx.CreateResponseAsync("Paused");
    }
}