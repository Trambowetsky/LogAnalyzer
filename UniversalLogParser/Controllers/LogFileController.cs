using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversalLogParser.Data;
using UniversalLogParser.Models;
using UniversalLogParser.Services;

namespace UniversalLogParser.Controllers;

public class LogFilesController : Controller
{
    private readonly AppDbContext _context;
    private readonly ILogParserService _parser;

    public LogFilesController(AppDbContext context, ILogParserService parser)
    {
        _context = context;
        _parser = parser;
    }
    public async Task<IActionResult> Index()
    {
        var files = await _context.LogFiles
            .Select(f => new LogFileListViewModel
            {
                Id = f.Id,
                FileName = f.FileName,
                UploadedOn = f.UploadedOn,
                Count = f.Entries.Count
            })
            .ToListAsync();

        return View(files);
    }

    public async Task<IActionResult> Details(int id)
    {
        var file = await _context.LogFiles
            .Include(f => f.Entries)
            .FirstOrDefaultAsync(f => f.Id == id); 

        if (file == null) return NotFound();

        return View(file);
    }

    [HttpGet]
    public IActionResult Upload()
    {
        return View();
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

    protected virtual async Task<int> CreateLogFileOnUploading(IFormFile file)
    {
        int fileId = CheckIfLogFileExists(file);
        if (fileId > 0)
            return fileId;
        var logFile = new LogFile {
            FileName = file.FileName,
            UploadedOn = DateTime.UtcNow,
            FileSize = file.Length
        };
        _context.LogFiles.Add(logFile);
        await _context.SaveChangesAsync();
        
        return logFile.Id;
    }

    protected virtual int CheckIfLogFileExists(IFormFile file)
    {
        var fileId = _context.LogFiles
            .Where(x => x.FileName == file.FileName)
            .Where(y => y.FileSize == file.Length)
            .Select(x => x.Id)
            .FirstOrDefault();
        
        return fileId;
    }
    public async Task<IActionResult> Delete(int id)
    {
        var file = await _context.LogFiles.Include(f => f.Entries)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (file == null)
            return NotFound();

        _context.LogEntries.RemoveRange(file.Entries);
        _context.LogFiles.Remove(file);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}