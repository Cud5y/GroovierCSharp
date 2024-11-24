namespace GroovierCSharp;

internal static class Program
{
    private static async Task Main(string[] _)
    {
        await Setup.Run();
        await Task.Delay(-1);
    }
}