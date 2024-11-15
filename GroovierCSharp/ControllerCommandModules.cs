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
            var node = vnext.ConnectedNodes.Values.First();
            var channel = ctx.Member.VoiceState.Channel;
            await node.ConnectAsync(channel);
            await ctx.CreateResponseAsync($"Joined {channel.Name}");
        }
        catch (Exception e)
        {
            await ctx.CreateResponseAsync("You need to be in a voice channel.");
        }
    }

    [SlashCommand("leave", "Leaves the voice channel")]
    public async Task leave(InteractionContext ctx)
    {
        var vnext = ctx.Client.GetLavalink();
        var node = vnext.ConnectedNodes.Values.First();
        var connection = node.GetGuildConnection(ctx.Guild);
        await connection.DisconnectAsync();
        await ctx.CreateResponseAsync($"Left {connection.Channel.Name}");
    }
}