using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Markdig;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
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
    public static Post? GeneratePostFromFile(string relPath, MarkdownPipeline pipeline, IDeserializer yamlDeserializer) {
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
        string text = File.ReadAllText(ToAbs(relPath));
        
        // sanity check
        if (!text.StartsWith("---")) {
            Log.E($"No front matter found in {relPath}");
            return null;
        }

        // gotta find all of these (except dates, they will be extracted from file info)
        string? title = null;
        string? category = null;
        string? excerpt = null;
        DateTimeOffset created = DateTimeOffset.MinValue;
        DateTimeOffset modified = DateTimeOffset.MinValue;

        string frontMatterText = text.Substring(3, text.IndexOf("---", 3) - 3).Trim();
        var frontMatter = yamlDeserializer.Deserialize<FrontMatter>(frontMatterText);

        if (frontMatter.published == false) return null;

        title = frontMatter.title;
        category = frontMatter.categories.First() ?? "미분류";
        excerpt = frontMatter.excerpt;
        created = frontMatter.date ?? File.GetCreationTime(ToAbs(relPath));
        modified = frontMatter.last_modified_at ?? File.GetLastWriteTime(ToAbs(relPath));

        created = new DateTimeOffset(created.Date, created.Offset);
        modified = new DateTimeOffset(modified.Date, modified.Offset);

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
            DateCreated = created,
            DateModified = modified,
            Category = category,
            Excerpt = excerpt,
        };

        // -- process body --
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

    public class FrontMatter {
        public required string title { get; set; }
        public required DateTimeOffset? date { get; set; }
        public required DateTimeOffset? last_modified_at { get; set; }
        public required string[] categories { get; set; }
        public required string excerpt { get; set; }
        public required bool published { get; set; } = true;
    }
}