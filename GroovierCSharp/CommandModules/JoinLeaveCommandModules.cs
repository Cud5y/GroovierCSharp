using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;

namespace GroovierCSharp;

public class JoinLeaveCommandModules : ApplicationCommandModule
{
    [SlashCommand("ping", "Replies with Pong!")]
    public static async Task Ping(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync("Pong!");
    }

    [SlashCommand("Join", "Joins the voice channel")]
    public static async Task Join(InteractionContext ctx)
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

    [SlashCommand("Leave", "Leaves the voice channel")]
    public static async Task Leave(InteractionContext ctx)
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

        ControllerCommandModules.Queue.Clear();
        await connection.DisconnectAsync();
        await ctx.CreateResponseAsync($"Left {connection.Channel.Name}");
    }
}