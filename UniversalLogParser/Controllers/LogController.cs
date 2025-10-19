using Microsoft.AspNetCore.Mvc;
using UniversalLogParser.Data;
using UniversalLogParser.Models;
using UniversalLogParser.Services;

namespace UniversalLogParser.Controllers;

public class LogController : Controller
{
    private readonly AppDbContext _context;
    private readonly ILogParserService _parser;
    
    public LogController(AppDbContext context, ILogParserService parser)
    {
        _context = context;
        _parser = parser;
    }
    [HttpGet]
    public IActionResult Index(int id)
    {
        var logs = _context.LogEntries
            .Where(x => x.LogFileId == id)
            .OrderByDescending(x => x.Date)
            .Take(100)
            .ToList();
        return View(logs);
    }
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file != null && file.Length > 0)
        {
            var tempPath = Path.GetTempFileName();
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            int fileId = await CreateLogFileOnUploading(file);
            var entries = _parser.Parse(tempPath, fileId);
            _context.LogEntries.AddRange(entries);
            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }

    protected async virtual Task<int> CreateLogFileOnUploading(IFormFile file)
    {
        var logFile = new LogFile {
            FileName = file.FileName,
            UploadedOn = DateTime.UtcNow
        };
        _context.LogFiles.Add(logFile);
        await _context.SaveChangesAsync();
        
        return logFile.Id;
    }
}