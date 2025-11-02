using Microsoft.EntityFrameworkCore;
using UniversalLogParser.Models;

namespace UniversalLogParser.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<LogEntry> LogEntries { get; set; }
    
    public DbSet<LogFile> LogFiles { get; set; }
    
    public DbSet<UploadedLogFile> UploadedLogFiles { get; set; }
}