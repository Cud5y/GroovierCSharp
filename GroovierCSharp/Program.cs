using DSharpPlus;
using DSharpPlus.SlashCommands;

namespace GroovierCSharp;

internal static class Program
{
    private static async Task Main(string[] args)
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
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
        };

        var client = new DiscordClient(config);
        var commands = client.UseSlashCommands();
        commands.RegisterCommands<ControllerCommandModules>(880830252740390992);
        await client.ConnectAsync();
        await Task.Delay(-1);
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
                trimmedLine.StartsWith('#')) continue; // Skip empty lines or comments

            var parts = trimmedLine.Split('=', 2); // Split only on the first '='
            if (parts.Length == 2)
            {
                var key = parts[0].Trim();
                var value = parts[1].Trim();

                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}