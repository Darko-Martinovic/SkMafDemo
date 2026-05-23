namespace SkMafDemo.Console.Ui;

// Tiny output helper. Centralised so every demo prints with consistent visual
// vocabulary: cyan headers, dimmed bracketed tool traces, default colour for
// model text. Nothing here is clever — the point is just that the audience can
// distinguish "model output" from "framework plumbing" at a glance.
public static class ConsoleUi
{
    public static void Header(string title, string subtitle)
    {
        var prev = System.Console.ForegroundColor;
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine();
        System.Console.WriteLine("==== " + title + " ====");
        System.Console.ForegroundColor = ConsoleColor.DarkCyan;
        System.Console.WriteLine(subtitle);
        System.Console.ForegroundColor = prev;
        System.Console.WriteLine();
    }

    public static void Info(string text)
    {
        var prev = System.Console.ForegroundColor;
        System.Console.ForegroundColor = ConsoleColor.DarkYellow;
        System.Console.WriteLine(text);
        System.Console.ForegroundColor = prev;
    }

    public static void Trace(string text)
    {
        var prev = System.Console.ForegroundColor;
        System.Console.ForegroundColor = ConsoleColor.DarkGray;
        System.Console.WriteLine("[trace] " + text);
        System.Console.ForegroundColor = prev;
    }

    public static void Warn(string text)
    {
        var prev = System.Console.ForegroundColor;
        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine(text);
        System.Console.ForegroundColor = prev;
    }

    public static void Error(string text)
    {
        var prev = System.Console.ForegroundColor;
        System.Console.ForegroundColor = ConsoleColor.Red;
        System.Console.WriteLine(text);
        System.Console.ForegroundColor = prev;
    }

    public static void Bullet(string text)
    {
        System.Console.WriteLine("  - " + text);
    }

    public static void PauseForMenu()
    {
        System.Console.WriteLine();
        System.Console.ForegroundColor = ConsoleColor.DarkGray;
        System.Console.WriteLine("(Press Enter to return to the menu.)");
        System.Console.ResetColor();
        // ReadLine() returns null on stdin EOF; that just means we're not interactive.
        System.Console.ReadLine();
    }
}
