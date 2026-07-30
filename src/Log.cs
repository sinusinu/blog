namespace Siblsenki;

public static class Log {
    public static void I(string content) {
        var oc = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine($"[I] {content}");
        Console.ForegroundColor = oc;
    }
    public static void W(string content) {
        var oc = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"[W] {content}");
        Console.ForegroundColor = oc;
    }
    public static void E(string content) {
        var oc = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[E] {content}");
        Console.ForegroundColor = oc;
    }
}