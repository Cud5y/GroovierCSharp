using DSharpPlus;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using DSharpPlus.SlashCommands;
using GroovierCSharp.CommandModules;

namespace GroovierCSharp;

public class Setup
{
    public static async Task Run()
    {
        LoadEnvFile(".env");
        var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("DISCORD_TOKEN environment variable is not set.");
            return;
        }

        Console.WriteLine($"Logging in with token: {token}");

        var config = new DiscordConfiguration
        {
            Token = token,
            TokenType = TokenType.Bot,
            AutoReconnect = true,
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildVoiceStates | DiscordIntents.GuildMessages |
                      DiscordIntents.Guilds
        };

        var endpoint = new ConnectionEndpoint
        {
            Hostname = "127.0.0.1",
            Port = 2333
        };

        var lavalinkConfig = new LavalinkConfiguration
        {
            Password = "youshallnotpass",
            RestEndpoint = endpoint,
            SocketEndpoint = endpoint
        };

        var client = new DiscordClient(config);
        var lavalink = client.UseLavalink();
        var commands = client.UseSlashCommands();
        commands.RegisterCommands<ControllerCommandModules>();
        commands.RegisterCommands<JoinLeaveCommandModules>();
        commands.RegisterCommands<QueueControlCommandModules>();
        commands.RegisterCommands<PlaybackControlCommandModules>();
        await client.ConnectAsync();
        await lavalink.ConnectAsync(lavalinkConfig);
    }

    private static void LoadEnvFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Error: {filePath} file not found.");
            return;
        }

        foreach (var line in File.ReadAllLines(filePath))
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine) ||
                trimmedLine.StartsWith('#')) continue;

            var parts = trimmedLine.Split('=', 2);
            if (parts.Length == 2)
            {
                var key = parts[0].Trim();
                var value = parts[1].Trim();

                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}