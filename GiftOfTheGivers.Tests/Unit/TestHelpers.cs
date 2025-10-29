using Microsoft.Extensions.Logging;

public static class TestHelpers
{
    public static ILogger<T> CreateStubLogger<T>()
    {
        return LoggerFactory.Create(builder => { }).CreateLogger<T>();
    }
}