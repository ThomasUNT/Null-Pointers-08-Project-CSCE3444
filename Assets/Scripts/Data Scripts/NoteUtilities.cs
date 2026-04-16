using System.IO;

public static class NoteUtilities
{
    public static string FindFilePathById(string noteFolder, string targetGuid)
    {
        // Get all .md files in the folder and subfolders
        string[] files = Directory.GetFiles(noteFolder, "*.md", SearchOption.AllDirectories);

        foreach (string path in files)
        {
            string content = File.ReadAllText(path);
            // Simple check to see if the ID exists in the header
            if (content.Contains($"ID: {targetGuid}"))
            {
                return path;
            }
        }
        return null;
    }
}