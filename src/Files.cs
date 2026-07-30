using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Siblsenki;

public static class Files {
    public static string ToAbs(string relPath) => Path.Combine(Environment.CurrentDirectory, relPath);
    
    public static bool LoadJson(string relPath, [NotNullWhen(true)] out JsonDocument? jsonDocument) {
        jsonDocument = null;
        var exists = File.Exists(ToAbs(relPath));
        if (!exists) return false;
        try { jsonDocument = JsonDocument.Parse(File.ReadAllText(relPath)); } catch (Exception e) { Console.WriteLine(e.StackTrace); }
        return jsonDocument != null;
    }
}