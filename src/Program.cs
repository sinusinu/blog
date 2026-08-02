using System.Collections.Immutable;
using System.Globalization;
using System.Net;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using Markdig;
using YamlDotNet.Serialization;
using static Siblsenki.Files;

namespace Siblsenki;

class Program {
    static DateTimeOffset? _generateTime;
    static DateTimeOffset GenerateTime => _generateTime ?? DateTimeOffset.Now;

    static int postPerPage = 1;

    static void Main(string[] args) {
        _generateTime = DateTimeOffset.Now;

        // nuke dist if exists
        if (Directory.Exists(ToAbs("dist"))) Directory.Delete(ToAbs("dist"), true);
        Directory.CreateDirectory(ToAbs("dist"));

        // quick sanity check
        if (!Directory.Exists(ToAbs(Path.Combine("posts", "assets")))) { Panic("Directory 'posts/assets' does not exist!"); return; }
        if (!Directory.Exists(ToAbs(Path.Combine("posts", "texts"))))  { Panic("Directory 'posts/texts' does not exist!"); return; }
        if (!Directory.Exists(ToAbs("skel")))                          { Panic("Directory 'skel' does not exist!"); return; }

        var mdPipeline = new MarkdownPipelineBuilder()
            .UseAlertBlocks(renderKind: (renderer, kind) => {
                renderer.Write($"<p class=\"markdown-alert-title\"><span class=\"material-symbols-outlined icon-right-padding\">{GetAlertBlockKindIcon(kind.ToString())}</span>{GetAlertBlockKindName(kind.ToString())}</p>");
            })
            .UseEmphasisExtras()
            .UseFootnotes()
            .UseTaskLists()
            .UsePipeTables()
            .UseMediaLinks()
            .UseCjkFriendlyEmphasis()
            .UseGenericAttributes()
            .Build();

        var yamlDeserializer = new DeserializerBuilder()
            .Build();

        // read all posts/texts
        Log.I("Parsing posts...");
        var postTextFiles = Directory.GetFiles(ToAbs(Path.Combine("posts", "texts")), "*.md");
        List<Post> posts = new();
        foreach (var postTextFile in postTextFiles) {
            var post = Post.GeneratePostFromFile(postTextFile, mdPipeline, yamlDeserializer);
            if (post is not null) posts.Add(post);
        }
        if (posts.Count == 0) { Panic("No posts found!"); return; }
        posts.Sort((Post x, Post y) => { return x.Head.DateCreated.CompareTo(y.Head.DateCreated); });
        
        // load and parse skeleton head
        if (!LoadJson(Path.Combine("skel", "_head.json"), out var skelHead)) { Panic("File 'skel/_head.json' does not exist or malformed!"); return; }

        string[]? dynamicTargets = null;
        string[]? staticTargets = null;
        PostCopyOps[]? postCopyOps = null;
        try {
            dynamicTargets = skelHead!.RootElement.GetProperty("dynamicTargets").EnumerateArray().Select(x => x.GetString()).ToArray()!;
            staticTargets = skelHead!.RootElement.GetProperty("staticTargets").EnumerateArray().Select(x => x.GetString()).ToArray()!;
            postCopyOps = skelHead!.RootElement.GetProperty("postCopyOps").EnumerateArray().Select(x => new PostCopyOps(x.GetProperty("src").GetString()!, x.GetProperty("dst").GetString()!)).ToArray()!;
        } catch (Exception e) {
            Panic($"Failed to parse skeleton head! {e}");
            return;
        }

        if (dynamicTargets is null || staticTargets is null || postCopyOps is null) {
            Panic($"Failed to retrieve something from skeleton head?");
            return;
        }

        foreach (var option in skelHead.RootElement.GetProperty("options").EnumerateObject()) {
            switch (option.Name) {
                case "postPerPage":
                    postPerPage = option.Value.GetInt32();
                    break;
            }
        }

        // generate dynamic targets
        Log.I("Generating dynamic targets...");
        foreach (var dynamicTarget in dynamicTargets) {
            Log.I($"Parsing {dynamicTarget}");
            var raw = File.ReadAllText(Path.Combine("skel", dynamicTarget));
            var dt = JsonSerializer.Deserialize<DynamicTarget>(raw)!;
            switch (dt.Type) {
                case "post":
                    GeneratePosts(dt, posts);
                    break;
                case "list":
                    GenerateLists(dt, posts);
                    break;
                case "category":
                    GenerateCategories(dt, posts);
                    break;
                default:
                    Log.E($"Unknown dynamic target type {dt.Type}");
                    break;
            }
        }

        // copy static targets
        Log.I("Copying static targets...");
        foreach (var staticTarget in staticTargets) {
            try {
                Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine("dist", staticTarget))!);
                File.Copy(Path.Combine("skel", staticTarget), Path.Combine("dist", staticTarget));
                Log.I($"Copied {staticTarget}");
            } catch (Exception e) {
                Log.E($"Failed to copy {staticTarget}! {e.Message}");
            }
        }

        // copy post assets
        Log.I("Copying post assets...");
        Directory.CreateDirectory(Path.Combine("dist", "assets"));
        Utils.CopyDirectory(Path.Combine("posts", "assets"), Path.Combine("dist", "assets"), true);
        Log.I("Copied post assets");

        // perform post copy ops
        Log.I("Performing post copy ops...");
        foreach (var postCopyOp in postCopyOps) {
            try {
                Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine("dist", postCopyOp.dst))!);
                File.Copy(Path.Combine("dist", postCopyOp.src), Path.Combine("dist", postCopyOp.dst));
                Log.I($"CopyOp succeeded for {postCopyOp.src}");
            } catch (Exception e) {
                Log.E($"CopyOp failed for {postCopyOp.src}! {e.Message}");
            }
        }
        
        Log.I("Done!");
    }

    static void GeneratePosts(DynamicTarget target, List<Post> posts) {
        Log.I($"Opening skeleton file {target.In}");
        string skel = File.ReadAllText(Path.Combine("skel", target.In));
        foreach (var post in posts) {
            Log.I($"Generating file for post: {post.Path}");

            // resolve path
            var path = ResolveMarkedKeys(target.Out, ResolverHintType.Post, post);

            // resolve content
            var content = ResolveMarkedKeys(skel, ResolverHintType.Post, post);

            // save file
            Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine("dist", path.ToString()))!);
            File.WriteAllText(Path.Combine("dist", path.ToString()), content);
        }
    }
    
    static void GenerateLists(DynamicTarget target, List<Post> posts) {
        Log.I("Figuring out lists");
        List<List<Post>> lists = new();
        List<Post> currentPage = new();
        for (int i = posts.Count - 1; i >= 0; i--) {
            currentPage.Add(posts[i]);
            if (currentPage.Count == postPerPage) {
                lists.Add(currentPage);
                currentPage = new();
            }
        }
        if (currentPage.Count > 0) lists.Add(currentPage);

        Log.I($"Opening skeleton file {target.In}");
        string skel = File.ReadAllText(Path.Combine("skel", target.In));

        for (int i = 0; i < lists.Count; i++) {
            Log.I($"Generating file for list #{i + 1}");

            // resolve path
            var path = ResolveMarkedKeys(target.Out, ResolverHintType.List, (lists, i));

            Log.I($"target: {path}");

            // resolve content
            var content = ResolveMarkedKeys(skel, ResolverHintType.List, (lists, i));

            // save file
            Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine("dist", path.ToString()))!);
            File.WriteAllText(Path.Combine("dist", path.ToString()), content);
        }

        if (target.Options is not null && target.Options.ContainsKey("firstPageAsParentIndex") && (target.Options["firstPageAsParentIndex"].ValueKind == JsonValueKind.Object)) {
            Log.I("Generating first page list as parent index");

            var indexIn = target.Options["firstPageAsParentIndex"].GetProperty("in").GetString()!;
            var indexOut = target.Options["firstPageAsParentIndex"].GetProperty("out").GetString()!;
            
            skel = File.ReadAllText(Path.Combine("skel", indexIn));

            // resolve path
            var path = ResolveMarkedKeys(indexOut, ResolverHintType.List, (lists, 0));
            
            Log.I($"target: {path}");

            // resolve content
            var content = ResolveMarkedKeys(skel, ResolverHintType.List, (lists, 0));

            // save file
            Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine("dist", path.ToString()))!);
            File.WriteAllText(Path.Combine("dist", path.ToString()), content);
        }
    }

    static void GenerateCategories(DynamicTarget target, List<Post> posts) {
        Log.I("Figuring out categories");
        var categories = new Dictionary<string, List<Post>>();
        for (int i = 0; i < posts.Count; i++) {
            if (!categories.ContainsKey(posts[i].Head.Category)) categories.Add(posts[i].Head.Category, new List<Post>());
            categories[posts[i].Head.Category].Add(posts[i]);
        }
        List<string> categoryNames = categories.Keys.ToList();
        categoryNames.Sort();

        Log.I($"Found {categoryNames.Count} categories");

        if (target.Options is null || !target.Options.ContainsKey("categoryList") || !target.Options.ContainsKey("categoryListIndex")) {
            Log.E($"Required options missing in category target!");
            return;
        }

        Log.I("Generating category lists");
        {
            string targetIn = target.Options["categoryList"].GetProperty("in").GetString()!;
            string targetOut = target.Options["categoryList"].GetProperty("out").GetString()!;
            string targetIndexIn = target.Options["categoryListIndex"].GetProperty("in").GetString()!;
            string targetIndexOut = target.Options["categoryListIndex"].GetProperty("out").GetString()!;

            Log.I($"Opening skeleton file {targetIn}");
            string skel = File.ReadAllText(Path.Combine("skel", targetIn));
            string skelIndex = File.ReadAllText(Path.Combine("skel", targetIndexIn));

            foreach (var categoryName in categoryNames) {
                Log.I($"Generating lists for category {categoryName}");
                var targetCategory = categories[categoryName];
                
                Log.I("Figuring out lists");
                List<List<Post>> lists = new();
                List<Post> currentPage = new();
                for (int i = targetCategory.Count - 1; i >= 0; i--) {
                    currentPage.Add(targetCategory[i]);
                    if (currentPage.Count == postPerPage) {
                        lists.Add(currentPage);
                        currentPage = new();
                    }
                }
                if (currentPage.Count > 0) lists.Add(currentPage);

                for (int i = 0; i < lists.Count; i++) {
                    Log.I($"Generating file for list #{i} of category {categoryName}");
                    
                    // resolve path
                    var path = ResolveMarkedKeys(targetOut, ResolverHintType.CategoryList, (lists, i, categoryName));
                    
                    Log.I($"target: {path}");

                    // resolve content
                    var content = ResolveMarkedKeys(skel, ResolverHintType.CategoryList, (lists, i, categoryName));

                    // save file
                    Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine("dist", path.ToString()))!);
                    File.WriteAllText(Path.Combine("dist", path.ToString()), content);
                }

                Log.I($"Generating first page list of category {categoryName} as index");
                    
                // resolve path
                var indexPath = ResolveMarkedKeys(targetIndexOut, ResolverHintType.CategoryList, (lists, 0, categoryName));
                
                Log.I($"target: {indexPath}");

                // resolve content
                var indexContent = ResolveMarkedKeys(skelIndex, ResolverHintType.CategoryList, (lists, 0, categoryName));

                // save file
                Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine("dist", indexPath.ToString()))!);
                File.WriteAllText(Path.Combine("dist", indexPath.ToString()), indexContent);
            }
        }

        Log.I("Generating category index");
        {
            Log.I("Figuring out category indices");
            List<(string, int)> categoryIndices = categoryNames.Select(m => (m, categories[m].Count)).ToList();

            Log.I($"Opening skeleton file {target.In}");
            string skel = File.ReadAllText(Path.Combine("skel", target.In));
            
            Log.I($"Generating category index page");
                    
            // resolve path
            var indexPath = ResolveMarkedKeys(target.Out, ResolverHintType.CategoryIndex, categoryIndices);
            
            Log.I($"target: {indexPath}");

            // resolve content
            var indexContent = ResolveMarkedKeys(skel, ResolverHintType.CategoryIndex, categoryIndices);

            // save file
            Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine("dist", indexPath.ToString()))!);
            File.WriteAllText(Path.Combine("dist", indexPath.ToString()), indexContent);
        }
    }

    enum ResolverHintType { Post, List, CategoryList, CategoryIndex }

    /// <param name="resolverHint">
    ///    hint type                       / resolverHintType
    ///    Post                            / ResolverHintType.Post
    ///    (List<List<Post>>, int)         / ResolverHintType.List
    ///    (List<List<Post>>, int, string) / ResolverHintType.CategoryList
    ///    (List<string>, int)             / ResolverHintType.CategoryIndex
    /// </param>
    static string ResolveMarkedKeys(string source, ResolverHintType resolverHintType, object resolverHint) {
        var sb = new StringBuilder();
        int pos = 0;
        while (pos < source.Length) {
            int start = source.IndexOf("%%", pos, StringComparison.Ordinal);
            if (start == -1) {
                // done, append the rest
                sb.Append(source, pos, source.Length - pos);
                break;
            }

            // found the marker - append everything else before the marker
            sb.Append(source, pos, start - pos);

            int end = source.IndexOf("%%", start + 2, StringComparison.Ordinal);
            if (end == -1) {
                // was unterminated marker - just append remaining as is
                sb.Append(source, start, source.Length - start);
                break;
            }

            string key = source.Substring(start + 2, end - start - 2);
            string resolved = key;
            switch (resolverHintType) {
                case ResolverHintType.Post:
                    if (resolverHint is Post) resolved = ResolveMarkedKeyPost(key, (Post)resolverHint);
                    break;
                case ResolverHintType.List:
                    if (resolverHint is (List<List<Post>>, int)) {
                        var (list, listIndex) = ((List<List<Post>>, int))resolverHint;
                        resolved = ResolveMarkedKeyList(key, list, listIndex);
                    }
                    break;
                case ResolverHintType.CategoryList:
                    if (resolverHint is (List<List<Post>>, int, string)) {
                        var (list, listIndex, categoryName) = ((List<List<Post>>, int, string))resolverHint;
                        resolved = ResolveMarkedKeyCategoryList(key, list, listIndex, categoryName);
                    }
                    break;
                case ResolverHintType.CategoryIndex:
                    if (resolverHint is List<(string, int)>) {
                        var categoryIndices = (List<(string, int)>)resolverHint;
                        resolved = ResolveMarkedKeyCategoryIndex(key, categoryIndices);
                    }
                    break;
            }
            sb.Append(resolved);

            pos = end + 2;
        }
        return sb.ToString();
    }

    static string ResolveMarkedKeyPost(string key, Post post) {
        if (key.StartsWith("conditional")) {
            // conditional key
            // %%conditional???(condition)???(target)%%
            // => target if condition, empty if !condition
            var conditionalInst = key.Split("???", 3);
            if (conditionalInst.Length != 3) return key;

            var conditionalCondition = conditionalInst[1];
            var conditionalTarget = conditionalInst[2];

            switch (conditionalCondition) {
                case "created_eq_modified":
                    if (post.Head.DateCreated == post.Head.DateModified) {
                        return conditionalTarget;
                    } else {
                        return "";
                    }
                default:
                    return key;
            }
        } else {
            // simple replace key
            switch (key) {
                case "post.title":
                    return post.Head.Title;
                case "post.title.escaped":
                    return WebUtility.HtmlEncode(post.Head.Title);
                case "post.created.date":
                    return post.Head.DateCreated.ToString("yyyy. MM. dd", new CultureInfo("ko-KR"));
                case "post.modified.date":
                    return post.Head.DateModified.ToString("yyyy. MM. dd", new CultureInfo("ko-KR"));
                case "post.created.full":
                    return post.Head.DateCreated.ToString("yyyy. MM. dd tt hh:mm", new CultureInfo("ko-KR"));
                case "post.modified.full":
                    return post.Head.DateModified.ToString("yyyy. MM. dd tt hh:mm", new CultureInfo("ko-KR"));
                case "post.excerpt":
                    return post.Head.Excerpt;
                case "post.excerpt.escaped":
                    return WebUtility.HtmlEncode(post.Head.Excerpt);
                case "post.category":
                    return post.Head.Category;
                case "post.body":
                    return post.Body;
                case "post.path":
                    return post.Path;
                default:
                    return ResolveSimpleMarkedKeyCommon(key);
            }
        }
    }

    public static string ResolveMarkedKeyList(string key, List<List<Post>> lists, int listIndex) {
        if (key.StartsWith("iterate")) {
            // iterating key
            // %%iterate???(key)???(target, %0...)???(inbetween)%%
            // => foreach key: target(k0), inbetween, target(k1), inbetween, ..., target(kn)
            var iterateInst = key.Split("???", 4);
            if (iterateInst.Length != 4) return key;
            
            var iterateKey = iterateInst[1];
            var iterateTarget = iterateInst[2];
            var iterateInbetween = iterateInst[3];

            var sb = new StringBuilder();
            int iterateIndex = 0;
            switch (iterateKey) {
                case "list.pageitem":
                    while (true) {
                        string sr = new string(iterateTarget);
                        sr = sr.Replace("%0", lists[listIndex][iterateIndex].Head.Title);
                        sr = sr.Replace("%1", lists[listIndex][iterateIndex].Head.Excerpt);
                        sr = sr.Replace("%2", lists[listIndex][iterateIndex].Head.DateCreated.ToString("yyyy. MM. dd", new CultureInfo("ko-KR")));
                        sr = sr.Replace("%3", lists[listIndex][iterateIndex].Head.Category);
                        sr = sr.Replace("%4", $"{lists[listIndex][iterateIndex].Path}/");
                        sb.Append(sr);
                        iterateIndex++;
                        if (iterateIndex == lists[listIndex].Count) break;
                        else sb.Append(iterateInbetween);
                    }
                    return sb.ToString();
            }
            return key;
        } else if (key.StartsWith("repl-cond")) {
            // replacing conditional key
            // %%repl-cond???(condition)???(target, %0...)%%
            // => target(k0...) if condition, empty if !condition
            var conditionalInst = key.Split("???", 3);
            if (conditionalInst.Length != 3) return key;

            var conditionalCondition = conditionalInst[1];
            var conditionalTarget = conditionalInst[2];

            switch (conditionalCondition) {
                case "not_first_page":
                    if (listIndex > 0) {
                        return conditionalTarget.Replace("%0", listIndex.ToString());
                    } else {
                        return "";
                    }
                case "not_last_page":
                    if (listIndex < lists.Count - 1) {
                        return conditionalTarget.Replace("%0", (listIndex + 2).ToString());
                    } else {
                        return "";
                    }
                default:
                    return key;
            }
        } else {
            // simple replace key
            switch (key) {
                case "list.index.one":
                    return (listIndex + 1).ToString();
                case "list.index.one.total":
                    return lists.Count.ToString();
                default:
                    return ResolveSimpleMarkedKeyCommon(key);
            }
        }
    }

    public static string ResolveMarkedKeyCategoryList(string key, List<List<Post>> lists, int listIndex, string categoryName) {
        if (key.StartsWith("iterate")) {
            // iterating key
            // %%iterate???(key)???(target, %0...)???(inbetween)%%
            // => foreach key: target(k0), inbetween, target(k1), inbetween, ..., target(kn)
            var iterateInst = key.Split("???", 4);
            if (iterateInst.Length != 4) return key;
            
            var iterateKey = iterateInst[1];
            var iterateTarget = iterateInst[2];
            var iterateInbetween = iterateInst[3];

            var sb = new StringBuilder();
            int iterateIndex = 0;
            switch (iterateKey) {
                case "category.list.pageitem":
                    while (true) {
                        string sr = new string(iterateTarget);
                        sr = sr.Replace("%0", lists[listIndex][iterateIndex].Head.Title);
                        sr = sr.Replace("%1", lists[listIndex][iterateIndex].Head.Excerpt);
                        sr = sr.Replace("%2", lists[listIndex][iterateIndex].Head.DateCreated.ToString("yyyy. MM. dd", new CultureInfo("ko-KR")));
                        sr = sr.Replace("%3", lists[listIndex][iterateIndex].Head.Category);
                        sr = sr.Replace("%4", $"{lists[listIndex][iterateIndex].Path}/");
                        sb.Append(sr);
                        iterateIndex++;
                        if (iterateIndex == lists[listIndex].Count) break;
                        else sb.Append(iterateInbetween);
                    }
                    return sb.ToString();
            }
            return key;
        } else if (key.StartsWith("repl-cond")) {
            // replacing conditional key
            // %%repl-cond???(condition)???(target, %0...)%%
            // => target(k0...) if condition, empty if !condition
            var conditionalInst = key.Split("???", 3);
            if (conditionalInst.Length != 3) return key;

            var conditionalCondition = conditionalInst[1];
            var conditionalTarget = conditionalInst[2];

            switch (conditionalCondition) {
                case "not_first_page":
                    if (listIndex > 0) {
                        return conditionalTarget.Replace("%0", listIndex.ToString());
                    } else {
                        return "";
                    }
                case "not_last_page":
                    if (listIndex < lists.Count - 1) {
                        return conditionalTarget.Replace("%0", (listIndex + 2).ToString());
                    } else {
                        return "";
                    }
                default:
                    return key;
            }
        } else {
            // simple replace key
            switch (key) {
                case "category.list.name":
                    return categoryName;
                case "category.list.index.one":
                    return (listIndex + 1).ToString();
                case "category.list.index.one.total":
                    return lists.Count.ToString();
                default:
                    return ResolveSimpleMarkedKeyCommon(key);
            }
        }
    }

    public static string ResolveMarkedKeyCategoryIndex(string key, List<(string, int)> categoryIndices) {
        if (key.StartsWith("iterate")) {
            // iterating key
            // %%iterate???(key)???(target, %0...)???(inbetween)%%
            // => foreach key: target(k0), inbetween, target(k1), inbetween, ..., target(kn)
            var iterateInst = key.Split("???", 4);
            if (iterateInst.Length != 4) return key;
            
            var iterateKey = iterateInst[1];
            var iterateTarget = iterateInst[2];
            var iterateInbetween = iterateInst[3];

            var sb = new StringBuilder();
            int iterateIndex = 0;
            switch (iterateKey) {
                case "category.category":
                    while (true) {
                        string sr = new string(iterateTarget);
                        sr = sr.Replace("%0", categoryIndices[iterateIndex].Item1);
                        sr = sr.Replace("%1", categoryIndices[iterateIndex].Item2.ToString());
                        sb.Append(sr);
                        iterateIndex++;
                        if (iterateIndex == categoryIndices.Count) break;
                        else sb.Append(iterateInbetween);
                    }
                    return sb.ToString();
            }
            return key;
        } else {
            // simple replace key
            switch (key) {
                default:
                    return ResolveSimpleMarkedKeyCommon(key);
            }
        }
    }

    public static string ResolveSimpleMarkedKeyCommon(string key) {
        switch (key) {
            case "year":
                return GenerateTime.Year.ToString();
            case "gents":
                return GenerateTime.ToUnixTimeMilliseconds().ToString();
            default:
                return key;
        }
    }

    static string GetAlertBlockKindName(string kind) {
        return kind switch {
            "NOTE" => "알림",
            "TIP" => "정보",
            "IMPORTANT" => "중요",
            "WARNING" => "경고",
            "CAUTION" => "주의",
            _ => kind,
        };
    }

    static string GetAlertBlockKindIcon(string kind) {
        return kind switch {
            "NOTE" => "info",
            "TIP" => "info",
            "IMPORTANT" => "info",
            "WARNING" => "warning",
            "CAUTION" => "warning",
            _ => kind,
        };
    }

    static void Panic(string description) {
        Log.E(description);
        Environment.Exit(1);
    }

    record PostCopyOps(string src, string dst);
}