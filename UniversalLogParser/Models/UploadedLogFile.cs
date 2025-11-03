namespace UniversalLogParser.Models;

public class UploadedLogFile
{
    public int Id { get; set; }
    
    public LogFile LogFile { get; set; }
    
    public byte[] FileContent { get; set; }
}