using System;
using System.Security.Principal;

namespace phylogenetic_project.StaticMethods;

static class ConsoleProgress
{
    private static DateTime _startTime;
    public static int _barWidth = 40;

    public static int last_value = 0;
    public static int cached_results = 0;
    public static int total = 1;

    public static void Start() => _startTime = DateTime.Now;

    public static void PerformStep(int step, string label = "")
    {
        last_value += step;
        Write(last_value, label);
    }

    public static void Write(int current, string label = "")
    {
        Write(current, total, label);
    }

    public static void Write(int current, int total, string label = "")
    {
        ConsoleProgress.last_value = current;
        ConsoleProgress.total = total;

        current -= cached_results;
        
        double pct = (double)(current) / total;
        int filled = (int)(pct * _barWidth);

        string eta = "";
        if (current > 0)
        {
            var elapsed = DateTime.Now - _startTime;
            var totalEstimated = TimeSpan.FromSeconds(elapsed.TotalSeconds / pct);
            var remaining = totalEstimated - elapsed;
            eta = $" ETA: {FormatTime(remaining)}";
        }

        string elapsed2 = FormatTime(DateTime.Now - _startTime);
        string bar = $"\r[{new string('#', filled)}{new string('-', _barWidth - filled)}] {pct:P0} | {current}/{total} | Elapsed: {elapsed2}{eta}";

        Console.WriteLine(bar);
        Console.WriteLine(label);
        Console.WriteLine();
    }

    private static string FormatTime(TimeSpan t) =>
        t.TotalHours >= 1
            ? $"{(int)t.TotalHours}h {t.Minutes:D2}m {t.Seconds:D2}s"
            : t.TotalMinutes >= 1
                ? $"{t.Minutes}m {t.Seconds:D2}s"
                : $"{t.Seconds}s";
}