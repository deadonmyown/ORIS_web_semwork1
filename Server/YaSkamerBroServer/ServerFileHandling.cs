using System.Text.Json;

namespace YaSkamerBroServer;

public class ServerFileHandling
{
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
}