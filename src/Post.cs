using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using Markdig;
using static Siblsenki.Files;

namespace Siblsenki;

public class Post {
    public required string Path { get; init; }
    public required PostHead Head { get; init; }
    public required string Body { get; init; }

    public class PostHead {
        public required string Title { get; init; }
        public required DateTimeOffset DateCreated { get; init; }
        public required DateTimeOffset DateModified { get; init; }
        public required string Category { get; init; }
        public required string Excerpt { get; init; }
    }

    /// <returns>`null` if file has `published` set to false.</returns>
    public static Post? GeneratePostFromFile(string relPath, MarkdownPipeline pipeline) {
        // get post path (filename without date)
        string fullFilename = System.IO.Path.GetFileNameWithoutExtension(ToAbs(relPath));
        string postPath = fullFilename;
        if (fullFilename.Length > 11 &&
            fullFilename[4] == '-' &&
            fullFilename[7] == '-' &&
            fullFilename[10] == '-') {
            postPath = fullFilename.Substring(11);
        }
        
        Log.I($"Parsing {fullFilename}");

        // -- iterate front matter --
        string[] lines = File.ReadAllLines(ToAbs(relPath));
        
        // sanity check
        if (lines[0] != "---") {
            Log.E($"No front matter found in {relPath}");
            return null;
        }

        // gotta find all of these (except dates, they will be extracted from file info)
        string? title = null;
        string? category = null;
        string? excerpt = null;
        DateTimeOffset? created = null;
        DateTimeOffset? modified = null;

        for (int i = 1; i < lines.Length; i++) {
            // reached end of front matter
            if (lines[i] == "---") break;

            // sanity check
            if (!lines[i].Contains(':')) {
                Log.E($"Malformed front matter found in {relPath}, line {i}");
                return null;
            }

            // TODO: do things with front matter
            string[] fm = lines[i].Split(':', 2, StringSplitOptions.TrimEntries);

            switch (fm[0]) {
                case "title":
                    title = fm[1].Trim('"');
                    break;
                case "categories":
                    category = fm[1].Trim('[', ']').Split(',', StringSplitOptions.TrimEntries).Select(s => s.Trim('"')).ToArray()[0];
                    break;
                case "excerpt":
                    excerpt = fm[1].Trim('"');
                    break;
                case "date":
                    created = DateTimeOffset.ParseExact(fm[1].Trim('"'), "yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture);
                    break;
                case "last_modified_at":
                    modified = DateTimeOffset.ParseExact(fm[1].Trim('"'), "yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture);
                    break;
                case "published":
                    if (fm[1] == "false") {
                        Log.W("Found post with published set to false, will be ignored");
                        return null;
                    }
                    break;
            }
        }

        if (title is null) {
            Log.E($"title not found in {relPath}");
            return null;
        }
        if (category is null) {
            Log.E($"categories not found in {relPath}");
            return null;
        }
        if (excerpt is null) {
            Log.E($"excerpt not found in {relPath}");
            return null;
        }

        PostHead postHead = new() {
            Title = title,
            DateCreated = created ?? File.GetCreationTime(ToAbs(relPath)),
            DateModified = modified ?? File.GetLastWriteTime(ToAbs(relPath)),
            Category = category,
            Excerpt = excerpt,
        };

        // -- process body --
        string text = File.ReadAllText(ToAbs(relPath));
        int textStart = text.IndexOf("---", 4) + 3;
        string body = text.Substring(textStart).Trim();

        // htmlize body
        var htmlBody = Markdown.ToHtml(body, pipeline);

        // pack it up
        return new Post() {
            Path = postPath,
            Head = postHead,
            Body = htmlBody,
        };
    }
}