using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;
using GroovierCSharp.Controllers;

namespace GroovierCSharp.CommandModules;

public class ControllerCommandModules : ApplicationCommandModule
{
    [SlashCommand("Pause", "Pauses the current song")]
    public static async Task Pause(InteractionContext ctx)
    {
        await ConnectionSetup(ctx);
        var connection = LavaLinkController.Connection[ctx.Guild.Id];
        await connection.PauseAsync();
        var embed = EmbedCreator("Paused", connection.CurrentState.CurrentTrack.Title);
        await ctx.CreateResponseAsync(embed);
    }

    [SlashCommand("Resume", "Resumes the current song")]
    public static async Task Resume(InteractionContext ctx)
    {
        await ConnectionSetup(ctx);
        var connection = LavaLinkController.Connection[ctx.Guild.Id];
        await connection.ResumeAsync();
        var embed = EmbedCreator("Resumed", connection.CurrentState.CurrentTrack.Title);
        await ctx.CreateResponseAsync(embed);
    }

    [SlashCommand("Stop", "Stops the current song")]
    public static async Task Stop(InteractionContext ctx)
    {
        await ConnectionSetup(ctx);
        var connection = LavaLinkController.Connection[ctx.Guild.Id];
        GuildQueueManager.TryGetQueue(ctx.Guild.Id, out var queue);
        queue.Clear();
        await connection.StopAsync();
        var embed = EmbedCreator("Stopped", connection.CurrentState.CurrentTrack.Title);
        await ctx.CreateResponseAsync(embed);
    }

    [SlashCommand("Skip", "Skips the current song")]
    public static async Task Skip(InteractionContext ctx)
    {
        await ConnectionSetup(ctx);
        var connection = LavaLinkController.Connection[ctx.Guild.Id];
        var embed = EmbedCreator("Skipping", connection.CurrentState.CurrentTrack.Title);
        await ctx.CreateResponseAsync(embed);
        await connection.StopAsync();
    }

    public static async Task ConnectionSetup(InteractionContext ctx)
    {
        LavaLinkController.Vnext = ctx.Client.GetLavalink();
        if (LavaLinkController.Vnext is null)
        {
            await ctx.CreateResponseAsync("Lavalink is not connected.");
            return;
        }

        LavaLinkController.Node = LavaLinkController.Vnext.ConnectedNodes.Values.First();
        if (LavaLinkController.Node is null)
        {
            await ctx.CreateResponseAsync("No connection nodes found.");
            return;
        }

        var connection = LavaLinkController.Connection[ctx.Guild.Id];

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (connection is null)
        {
            await ctx.CreateResponseAsync("No active connection found.");
            return;
        }

        if (connection.CurrentState.CurrentTrack is null)
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