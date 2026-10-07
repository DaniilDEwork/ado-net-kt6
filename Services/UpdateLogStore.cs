using System.Text.Json;
using Kt6UpdateApi.Models;

namespace Kt6UpdateApi.Services;

public class UpdateLogStore
{
    private readonly object _sync = new();
    private readonly string _path;
    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

    public UpdateLogStore(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var relativePath = configuration["LoggingFile"] ?? "App_Data/updates.jsonl";
        _path = Path.GetFullPath(relativePath, environment.ContentRootPath);
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
    }

    public void Write(UpdateLogEntry entry)
    {
        lock (_sync)
        {
            File.AppendAllText(_path, JsonSerializer.Serialize(entry, _options) + Environment.NewLine);
        }
    }

    public List<UpdateLogEntry> GetRecent()
    {
        lock (_sync)
        {
            if (!File.Exists(_path))
            {
                return new List<UpdateLogEntry>();
            }

            return File.ReadAllLines(_path)
                .TakeLast(100)
                .Select(line => JsonSerializer.Deserialize<UpdateLogEntry>(line, _options)!)
                .OrderByDescending(entry => entry.RequestTime)
                .ToList();
        }
    }
}
