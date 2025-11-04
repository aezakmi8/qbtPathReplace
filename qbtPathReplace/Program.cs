using System.Text.RegularExpressions;
using BencodeNET.Objects;
using BencodeNET.Parsing;

namespace qbtPathReplace;

internal class Program
{
    private static void Main(string[] args)
    {
#if DEBUG
        args =
        [
            @"C:\Users\Work\Downloads\qBittorrent_Backup\Local\qBittorrent\BT_backup",
            @"D:\Download",
            @"/downloads/ad_content",
            "true"
        ];
#endif

        if (args.Length < 4)
        {
            Console.WriteLine("Usage: qbtPathReplace <btBackupPath> <existingPath> <newPath> <targetPathLinux>");
            Console.WriteLine(
                "Example: qbtPathReplace \"C:\\Users\\Work\\Downloads\\qBittorrent_Backup\\Local\\qBittorrent\\BT_backup\" \"D:\\Download\" \"/downloads/ad_content\" true");
            return;
        }

        var btBackupPath = args[0];
        var existingPath = args[1];
        var newPath = args[2];
        var targetPathLinux = bool.Parse(args[3]);

        var fastResumeFiles = GetFastResumeFiles(btBackupPath);

        var counter = 0;
        foreach (var file in fastResumeFiles)
            if (UpdateSavePath(file, existingPath, newPath, targetPathLinux))
                counter++;

        Console.WriteLine($"Save paths updated. Count: {counter}");
    }

    private static List<string> GetFastResumeFiles(string path)
    {
        return Directory.EnumerateFiles(path, "*.fastresume").ToList();
    }

    private static bool UpdateSavePath(string filePath, string existingPath, string newPath, bool targetPathLinux)
    {
        try
        {
            var fileBytes = File.ReadAllBytes(filePath);
            var bDict = (BDictionary)new BencodeParser().Parse(fileBytes);

            var updated = false;
            string? oldSavePath = null;
            string? newSavePath = null;

            if (UpdatePath(bDict, "save_path", existingPath, newPath, targetPathLinux, out var oldSavePathBStr))
            {
                oldSavePath = oldSavePathBStr!;
                newSavePath = bDict["save_path"].ToString();
                updated = true;
            }

            if (UpdatePath(bDict, "qBt-savePath", existingPath, newPath, targetPathLinux, out var oldQBtSavePathBStr))
            {
                oldSavePath = oldQBtSavePathBStr!;
                newSavePath = bDict["qBt-savePath"].ToString();

                updated = true;
            }
            
            if (bDict.TryGetValue("mapped_files", out var mappedFilesObj) && mappedFilesObj is BList mappedFiles)
            {
                if (NormalizeMappedFiles(mappedFiles, targetPathLinux))
                {
                    bDict["mapped_files"] = mappedFiles;
                    
                    updated = true;
                }
            }
            
            if (!updated) return false;

            Console.WriteLine(
                $"In file {new FileInfo(filePath).Name}: Path was {oldSavePath}, now is {newSavePath}");

            WriteDictionary(bDict, filePath);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating file {filePath}: {ex.Message}");
            return false;
        }
    }

    private static void WriteDictionary(BDictionary bDict, string filePath)
    {
        using var memoryStream = new MemoryStream();
        bDict.EncodeTo(memoryStream).Flush();
        File.WriteAllBytes(filePath, memoryStream.ToArray());
    }

    private static bool UpdatePath(BDictionary bDict, string key, string existingPath, string newPath,
        bool targetPathLinux, out string? oldPathBStr)
    {
        oldPathBStr = null;
        if (!bDict.TryGetValue(key, out var pathValue) || pathValue is not BString currentPathBStr) return false;

        var currentPath = oldPathBStr = currentPathBStr.ToString();

        var normalizedExistingPath = NormalizePath(existingPath, targetPathLinux);
        var normalizedCurrentPath = NormalizePath(currentPath, targetPathLinux);

        var pattern = $"^{Regex.Escape(normalizedExistingPath)}(/|\\\\|$)";
        var match = Regex.Match(normalizedCurrentPath, pattern);

        if (!match.Success) return false;

        var relativePath = match.Length < normalizedCurrentPath.Length
            ? normalizedCurrentPath.Substring(match.Length).TrimStart('/', '\\')
            : string.Empty;

        var updatedPath = Path.Combine(newPath, relativePath);

        updatedPath = targetPathLinux ? updatedPath.Replace('\\', '/') : updatedPath.Replace('/', '\\');

        bDict[key] = new BString(updatedPath);
        return true;
    }

    private static string NormalizePath(string path, bool targetPathLinux)
    {
        var separator = targetPathLinux ? '/' : '\\';
        var altSeparator = targetPathLinux ? '\\' : '/';
        return path.Replace(altSeparator, separator);
    }
    
    private static bool NormalizeMappedFiles(BList mappedFiles, bool targetPathLinux)
    {
        var changed = false;

        for (int i = 0; i < mappedFiles.Count; i++)
        {
            if (mappedFiles[i] is BString entry)
            {
                var newEntry = targetPathLinux
                    ? entry.ToString().Replace('\\', '/')
                    : entry.ToString().Replace('/', '\\');

                if (newEntry != entry.ToString())
                {
                    mappedFiles[i] = new BString(newEntry);
                    changed = true;
                }
            }
        }

        return changed;
    }
}