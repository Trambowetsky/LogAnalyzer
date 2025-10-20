using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversalLogParser.Data;
using UniversalLogParser.Models;

namespace UniversalLogParser.Controllers;

public class LogFilesController : Controller
{
    private readonly AppDbContext _context;

    public LogFilesController(AppDbContext context)
    {
        _context = context;
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