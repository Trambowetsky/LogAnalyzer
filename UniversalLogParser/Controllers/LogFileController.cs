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
    private string[] allowedExtensions = new[] { ".log" };

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
            .OrderBy(x => x.UploadedOn)
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

    [HttpGet]
    public async Task<IActionResult> Stats()
    {
        return View();
    }

    public async Task<IActionResult> Download(int id)
    {
        var uploadedFile = await _context.UploadedLogFiles
            .Include(f => f.LogFile)
            .FirstOrDefaultAsync(f => f.LogFileId == id);

        if (uploadedFile == null || uploadedFile.FileContent == null || uploadedFile.FileContent.Length == 0)
        {
            return NotFound("File not found or empty.");
        }
        var fileName = uploadedFile.LogFile?.FileName ?? $"log_{id}.log";

        const string contentType = "text/plain";

        return File(uploadedFile.FileContent, contentType, fileName);
    }
    
    [HttpPost]
    public async Task<IActionResult> UploadMultiple(List<IFormFile> files)
    {
        
        if (files == null || files.Count == 0)
        {
            TempData["Error"] = "No files selected.";
            return RedirectToAction("Upload");
        }

        foreach (var file in files)
        {
            if (file.Length == 0)
                continue;
            
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension)) {
                if(files.Count > 1)
                    return StatusCode(409);
                continue;
            }

            
            var tempPath = Path.GetTempFileName();
            await using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            int fileId = await CreateLogFileOnUploading(file);
            
            var entries = _parser.Parse(tempPath, fileId);
            _context.LogEntries.AddRange(entries);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }
    protected virtual async Task<int> CreateLogFileOnUploading(IFormFile file)
    {
        var logFile = new LogFile {
            FileName = file.FileName,
            UploadedOn = DateTime.UtcNow,
            FileSize = file.Length
        };
        _context.LogFiles.Add(logFile);
        await _context.SaveChangesAsync();
        
        await UploadFile(file, logFile.Id);
        return logFile.Id;
    }

    protected virtual async Task<bool> UploadFile(IFormFile file, int id)
    {
        if (file == null || file.Length == 0)
            return false;
        try
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            
            var uploadedLogFile = new UploadedLogFile {
                LogFileId = id,
                FileContent = memoryStream.ToArray()
            };
            _context.UploadedLogFiles.Add(uploadedLogFile);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UploadFile] Error saving file '{file.FileName}': {ex.Message}");
            return false;
        }
    }

    // CheckIfFileExists not used right now
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