using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.SlashCommands;
using GroovierCSharp.Controllers;

namespace GroovierCSharp.CommandModules;

public class PlayCommandModules : ApplicationCommandModule
{
    private static Timer? _disconnectTimer;

    [SlashCommand("play", "Plays a song")]
    public static async Task Play(InteractionContext ctx, [Option("query", "The song to play")] string query)
    {
        LavaLinkController.Vnext = ctx.Client.GetLavalink();
        var node = LavaLinkController.Vnext?.ConnectedNodes.Values.FirstOrDefault();
        if (node == null)
        {
            await ctx.CreateResponseAsync("Lavalink is not connected or no connection nodes found.");
            return;
        }

        LavaLinkController.Connection = node.GetGuildConnection(ctx.Guild);
        if (LavaLinkController.Connection == null)
        {
            var channel = ctx.Member.VoiceState.Channel;
            await node.ConnectAsync(channel);
            LavaLinkController.Connection = node.GetGuildConnection(ctx.Guild);
        }

        LavaLinkController.Connection.PlaybackFinished += OnPlaybackFinished;
        try
        {
            LavalinkLoadResult loadResult;
            if (Uri.TryCreate(query, UriKind.Absolute, out var uri))
                loadResult = await node.Rest.GetTracksAsync(uri);
            else
                loadResult = await node.Rest.GetTracksAsync(query);

            await LoadFeedback(loadResult, ctx);
            var track = loadResult.Tracks.First();
            await PlaySong(track, ctx);
        }
        catch (Exception ex)
        {
            await ctx.CreateResponseAsync($"An error occurred: {ex.Message}");
        }
    }

    private static async Task LoadFeedback(LavalinkLoadResult loadResult, InteractionContext ctx)
    {
        switch (loadResult.LoadResultType)
        {
            case LavalinkLoadResultType.NoMatches:
                await ctx.CreateResponseAsync("No matches found for the query.");
                return;
            case LavalinkLoadResultType.LoadFailed:
                await ctx.CreateResponseAsync("Failed to load tracks.");
                return;
            case LavalinkLoadResultType.PlaylistLoaded:
                foreach (var playlistTrack in loadResult.Tracks)
                    GuildQueueManager.AddTrackToQueue(ctx.Guild.Id, playlistTrack);


                var playlistEmbed = ControllerCommandModules.EmbedCreator("Playlist Loaded",
                    $"Loaded playlist with {loadResult.Tracks.Count()} tracks.");
                await ctx.CreateResponseAsync(playlistEmbed);
                GuildQueueManager.TryDequeueTrack(ctx.Guild.Id, out var nextTrack);
                await PlaySong(nextTrack, ctx);
                return;
        }
    }

    private static async Task PlaySong(LavalinkTrack track, InteractionContext ctx)
    {
        if (LavaLinkController.Connection.CurrentState.CurrentTrack is not null)
        {
            GuildQueueManager.AddTrackToQueue(ctx.Guild.Id, track);
            var embed = ControllerCommandModules.EmbedCreator("Added to Queue", track.Title);
            await ctx.CreateResponseAsync(embed);
        }
        else
        {
            HistoryQueueManager.AddTrackToHistory(ctx.Guild.Id, track);
            await LavaLinkController.Connection.PlayAsync(track);
            var embed = ControllerCommandModules.EmbedCreator("Playing", track.Title);
            await ctx.CreateResponseAsync(embed);
            ResetDisconnectTimer(); // Reset the timer when a new song starts playing
        }
    }

    private static async Task OnPlaybackFinished(LavalinkGuildConnection sender, TrackFinishEventArgs e)
    {
        if (LavaLinkController.Connection.CurrentState.CurrentTrack is not null) return;
        if (LavaLinkController.Loop)
        {
            HistoryQueueManager.TryGetHistory(sender.Guild.Id, out var history);
            HistoryQueueManager.AddTrackToHistory(sender.Guild.Id, history.Last());
            await LavaLinkController.Connection.PlayAsync(history.Last());
            return;
        }

        if (GuildQueueManager.TryDequeueTrack(sender.Guild.Id, out var nextTrack))
        {
            HistoryQueueManager.AddTrackToHistory(sender.Guild.Id, nextTrack);
            await sender.PlayAsync(nextTrack);
        }
        else
        {
            StartDisconnectTimer(); // Start the timer when no more songs are in the queue
        }
    }

    private static void StartDisconnectTimer()
    {
        _disconnectTimer?.Dispose();
        _disconnectTimer = new Timer(DisconnectBot, null, TimeSpan.FromMinutes(15), Timeout.InfiniteTimeSpan);
    }

    private static void ResetDisconnectTimer()
    {
        _disconnectTimer?.Change(Timeout.Infinite, Timeout.Infinite);
    }

    private static void DisconnectBot(object? state)
    {
        LavaLinkController.Connection.DisconnectAsync();
        GuildQueueManager.TryGetQueue(LavaLinkController.Connection.Guild.Id, out var queue);
        queue.Clear();
        _disconnectTimer?.Dispose();
        _disconnectTimer = null;
        GC.Collect();
    }
}