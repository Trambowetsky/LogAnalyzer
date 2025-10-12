namespace UniversalLogParser.Models;

public class LogFile
{
    public int Id { get; set; }
    
    public string FileName { get; set; }
    
    public DateTime UploadedOn { get; set; }
    
    public List<LogEntry> Entries { get; set; } = new();
}