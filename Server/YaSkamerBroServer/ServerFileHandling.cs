using System.Text.Json;

namespace GameServer;

public class ServerFileHandling
{
    private static readonly string[] IndexFiles = {
        "index.html",
        "index.htm",
        "default.html",
        "default.htm"
    };
    
    public static (byte[], string) GetFile(string rawUrl, ServerSettings serverSettings)
    {
        byte[] buffer = null;
        string format = "";
        string filePath = serverSettings.Path + rawUrl;
        if (Directory.Exists(filePath))
        {
            filePath += "/index.html";
            if (File.Exists(filePath))
            {
                buffer = File.ReadAllBytes(filePath);
                format = filePath.Substring(filePath.LastIndexOf("."));
            }
        }
        else if (File.Exists(filePath))
        {
            buffer = File.ReadAllBytes(filePath);
            format = filePath.Substring(filePath.LastIndexOf("."));
        }

        Console.WriteLine(filePath);
        return (buffer, format);
    }
    
    public static (byte[], string) GetFileStatic(string filename, ServerSettings serverSettings, IDictionary<string, string> paths)
    {
        byte[] buffer = null;
        string format = "";

        if (string.IsNullOrEmpty(filename))
        {
            foreach (string indexFile in IndexFiles)
            {
                if (File.Exists(Path.Combine(serverSettings.Path, indexFile)))
                {
                    filename = Path.Combine(serverSettings.Path, indexFile);
                    buffer = File.ReadAllBytes(filename);
                    format = filename.Substring(filename.LastIndexOf("."));
                    break;
                }
            }
        }
        else if(paths.TryGetValue(filename, out string path))
        {
            buffer = File.ReadAllBytes(path);
            format = path.Substring(path.LastIndexOf("."));
            Console.WriteLine(path);
        }

        Console.WriteLine(filename);
        return (buffer, format);
    }

    public static ServerSettings ReadJsonSettings(string path)
    {
        if (File.Exists(path)) 
            return JsonSerializer.Deserialize<ServerSettings>(File.ReadAllBytes(path));
        else
        {
            Console.WriteLine($"Can't find settings at this path: {path}, program will use default server settings");
            return new ServerSettings();
        }
    }
    
    public static void ProcessDirectory(string targetDirectory, IDictionary<string, string> paths)
    {
        string[] fileEntries = Directory.GetFiles(targetDirectory);
        foreach (string fileName in fileEntries)
        {
            var tuple = ProcessFile(fileName);
            paths.Add(tuple.Item1, tuple.Item2);
        }

        string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
        foreach(string subdirectory in subdirectoryEntries)
            ProcessDirectory(subdirectory, paths);
        
    }
    
    public static (string, string) ProcessFile(string path)
    {
        Console.WriteLine($"Processed file '{path}'.");
        return (path.Split('\\', StringSplitOptions.RemoveEmptyEntries)[^1], path);
    }
}