namespace Kt6UpdateApi.Models;

public class UpdateLogEntry
{
    public DateTimeOffset RequestTime { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public bool Success { get; set; }
}
