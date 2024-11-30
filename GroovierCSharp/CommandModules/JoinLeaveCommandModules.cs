using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;

namespace GroovierCSharp.CommandModules;

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
            var embed = ControllerCommandModules.EmbedCreator("Join", $"Joined {channel.Name}");
            await ctx.CreateResponseAsync(embed);
        }
        catch (Exception)
        {
            var embed = ControllerCommandModules.EmbedCreator("Join", "You must be in a voice channel.");
            await ctx.CreateResponseAsync(embed);
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

        ControllerCommandModules.Queue.Clear();
        await ControllerCommandModules.Connection.DisconnectAsync();
        var embed = ControllerCommandModules.EmbedCreator("Join",
            $"Left {ControllerCommandModules.Connection.Channel.Name}");
        await ctx.CreateResponseAsync(embed);
    }
}