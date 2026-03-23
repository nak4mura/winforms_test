namespace WinFormsE2E.Core;

public static class WaitStrategy
{
    public static T? WaitUntil<T>(Func<T?> action, int timeoutMs, int intervalMs = 200) where T : class
    {
        var deadline = Environment.TickCount64 + timeoutMs;
        while (Environment.TickCount64 < deadline)
        {
            var result = action();
            if (result != null) return result;
            Thread.Sleep(intervalMs);
        }
        return null;
    }

    public static bool WaitUntilTrue(Func<bool> condition, int timeoutMs, int intervalMs = 200)
    {
        var deadline = Environment.TickCount64 + timeoutMs;
        while (Environment.TickCount64 < deadline)
        {
            if (condition()) return true;
            Thread.Sleep(intervalMs);
        }
        return false;
    }
}
