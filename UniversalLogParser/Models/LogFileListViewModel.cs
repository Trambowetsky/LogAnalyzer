namespace UniversalLogParser.Models;

public class LogFileListViewModel
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public DateTime UploadedOn { get; set; }
    public int Count { get; set; }
}