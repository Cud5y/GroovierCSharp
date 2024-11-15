using DSharpPlus;

namespace GroovierCSharp;

internal static class Program
{
    //dev branch
    private static async Task Main(string[] args)
    {
        var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
        var config = new DiscordConfiguration
        {
            Token = token,
            TokenType = TokenType.Bot,
            AutoReconnect = true,
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
        };

        var client = new DiscordClient(config);

        client.MessageCreated += async (s, e) =>
        {
            if (e.Message.Content.ToLower() == "ping") await e.Message.RespondAsync("Pong!");
        };

        await client.ConnectAsync();
        await Task.Delay(-1);
    }
}