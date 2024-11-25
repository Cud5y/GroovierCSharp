using System.Collections.Concurrent;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;

namespace GroovierCSharp.CommandModules;

public class PlaybackControlCommandModules : ApplicationCommandModule
{
    private static bool _boost;

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

    [SlashCommand("QueueLoop", "Loops the queue")]
    public static async Task QueueLoop(InteractionContext ctx)
    {
        await ControllerCommandModules.ConnectionSetup(ctx);
        ControllerCommandModules.QueueLoop = !ControllerCommandModules.QueueLoop;
        await ctx.CreateResponseAsync($"Queue looping set to {ControllerCommandModules.QueueLoop}");
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

    [SlashCommand("BassBoost", "Boosts the bass of the player")]
    public static async Task BassBoost(InteractionContext ctx)
    {
        await ControllerCommandModules.ConnectionSetup(ctx);
        _boost = !_boost;
        if (_boost)
        {
            await ControllerCommandModules.Connection.AdjustEqualizerAsync(new LavalinkBandAdjustment(0, 0.25f));
            await ctx.CreateResponseAsync("Bass boosted.");
            return;
        }

        await ControllerCommandModules.Connection.AdjustEqualizerAsync(new LavalinkBandAdjustment(0, 0));
        await ctx.CreateResponseAsync("Bass boost removed.");
    }
}