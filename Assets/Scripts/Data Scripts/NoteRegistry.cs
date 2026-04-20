using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public static class NoteRegistry
{
    // Maps NoteGUID -> FullDiskPath
    public static Dictionary<string, string> Cache = new Dictionary<string, string>();
    private static readonly Regex IdRegex = new Regex(@"^id:\s*(.+)$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

    public static void Rebuild(string folderPath)
    {
        Cache.Clear();
        if (!Directory.Exists(folderPath)) return;

        string[] files = Directory.GetFiles(folderPath, "*.md", SearchOption.AllDirectories);

        foreach (string path in files)
        {
            // Efficiency: Only read the top of the file for the ID
            string id = ExtractIdFromFrontmatter(path);
            if (!string.IsNullOrEmpty(id))
            {
                Cache[id] = path;
            }
        }
        Debug.Log($"Registry rebuilt. Found {Cache.Count} valid notes.");
    }

    private static string ExtractIdFromFrontmatter(string path)
    {
        using (var reader = new StreamReader(path))
        {
            for (int i = 0; i < 10; i++) // Check first 10 lines max
            {
                string line = reader.ReadLine();
                if (line == null) break;

                Match match = IdRegex.Match(line);
                if (match.Success) return match.Groups[1].Value.Trim();
            }
        }
        return null;
    }

    public static string GetPath(string guid)
    {
        if (string.IsNullOrEmpty(guid)) return null;
        return Cache.TryGetValue(guid, out string path) ? path : null;
    }

    public static void UpdateEntry(string id, string path)
    {
        if (string.IsNullOrEmpty(id)) return;

        if (Cache.ContainsKey(id))
        {
            Cache[id] = path;
        }
        else
        {
            Cache.Add(id, path);
        }
    }
}