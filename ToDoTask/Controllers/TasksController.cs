using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoTask.Date;
using ToDoTask.Models;

namespace ToDoTask.Controllers
{
    public class TasksController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        private const long MaxFileSize = 10 * 1024 * 1024;

        public TasksController(
            AppDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // Show all tasks
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var tasks = await _context.TaskItems
                .AsNoTracking()
                .OrderByDescending(t => t.Id)
                .ToListAsync();

            return View(tasks);
        }

        // Show Create page
        [HttpGet]
        public IActionResult Create()
        {
            return View(new TaskItem
            {
                Deadline = DateTime.Today
            });
        }

        // Create task and upload attachment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            TaskItem task,
            IFormFile? attachmentFile)
        {
            // Attachment is uploaded separately as IFormFile.
            ModelState.Remove(nameof(TaskItem.Attachment));

            if (attachmentFile != null &&
                attachmentFile.Length > MaxFileSize)
            {
                ModelState.AddModelError(
                    "attachmentFile",
                    "File size must not exceed 10 MB.");
            }

            if (!ModelState.IsValid)
            {
                return View(task);
            }

            string? savedFilePath = null;

            try
            {
                if (attachmentFile != null &&
                    attachmentFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(
                        _environment.ContentRootPath,
                        "wwwroot",
                        "uploads");

                    Directory.CreateDirectory(uploadsFolder);

                    string extension = Path.GetExtension(
                        Path.GetFileName(attachmentFile.FileName));

                    string fileName = $"{Guid.NewGuid():N}{extension}";

                    savedFilePath = Path.Combine(
                        uploadsFolder,
                        fileName);

                    await using (var stream = new FileStream(
                        savedFilePath,
                        FileMode.CreateNew))
                    {
                        await attachmentFile.CopyToAsync(stream);
                    }

                    task.Attachment = fileName;
                }
                else
                {
                    task.Attachment = null;
                }

                _context.TaskItems.Add(task);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                if (savedFilePath != null &&
                    System.IO.File.Exists(savedFilePath))
                {
                    System.IO.File.Delete(savedFilePath);
                }

                ModelState.AddModelError(
                    string.Empty,
                    "Could not save the task. Please check your database connection and task fields.");

                return View(task);
            }
        }

        // Show Edit page
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _context.TaskItems
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // Update task and optionally replace attachment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            TaskItem task,
            IFormFile? attachmentFile)
        {
            if (id != task.Id)
            {
                return NotFound();
            }

            var existingTask = await _context.TaskItems
                .FirstOrDefaultAsync(t => t.Id == id);

            if (existingTask == null)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(TaskItem.Attachment));

            if (attachmentFile != null &&
                attachmentFile.Length > MaxFileSize)
            {
                ModelState.AddModelError(
                    "attachmentFile",
                    "File size must not exceed 10 MB.");
            }

            if (!ModelState.IsValid)
            {
                task.Attachment = existingTask.Attachment;
                return View(task);
            }

            string? newFilePath = null;
            string? oldFilePath = null;

            try
            {
                if (attachmentFile != null &&
                    attachmentFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(
                        _environment.ContentRootPath,
                        "wwwroot",
                        "uploads");

                    Directory.CreateDirectory(uploadsFolder);

                    string extension = Path.GetExtension(
                        Path.GetFileName(attachmentFile.FileName));

                    string fileName = $"{Guid.NewGuid():N}{extension}";

                    newFilePath = Path.Combine(
                        uploadsFolder,
                        fileName);

                    await using (var stream = new FileStream(
                        newFilePath,
                        FileMode.CreateNew))
                    {
                        await attachmentFile.CopyToAsync(stream);
                    }

                    if (!string.IsNullOrWhiteSpace(
                        existingTask.Attachment))
                    {
                        oldFilePath = Path.Combine(
                            uploadsFolder,
                            Path.GetFileName(existingTask.Attachment));
                    }

                    existingTask.Attachment = fileName;
                }

                existingTask.Title = task.Title;
                existingTask.Description = task.Description;
                existingTask.Deadline = task.Deadline;

                await _context.SaveChangesAsync();

                if (oldFilePath != null &&
                    System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                if (newFilePath != null &&
                    System.IO.File.Exists(newFilePath))
                {
                    System.IO.File.Delete(newFilePath);
                }

                ModelState.AddModelError(
                    string.Empty,
                    "Could not update the task. Please try again.");

                task.Attachment = existingTask.Attachment;

                return View(task);
            }
        }

        // Download task attachment
        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var task = await _context.TaskItems
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound("Task not found.");
            }

            if (string.IsNullOrWhiteSpace(task.Attachment))
            {
                return NotFound("This task has no attachment.");
            }

            string uploadsFolder = Path.Combine(
                _environment.ContentRootPath,
                "wwwroot",
                "uploads");

            string fileName = Path.GetFileName(task.Attachment);
            string filePath = Path.Combine(uploadsFolder, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Attachment file not found.");
            }

            return PhysicalFile(
                filePath,
                "application/octet-stream",
                fileName);
        }

        // Delete task and its attachment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _context.TaskItems
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            string? filePath = null;

            if (!string.IsNullOrWhiteSpace(task.Attachment))
            {
                string uploadsFolder = Path.Combine(
                    _environment.ContentRootPath,
                    "wwwroot",
                    "uploads");

                filePath = Path.Combine(
                    uploadsFolder,
                    Path.GetFileName(task.Attachment));
            }

            _context.TaskItems.Remove(task);

            await _context.SaveChangesAsync();

            if (filePath != null &&
                System.IO.File.Exists(filePath))
            {
                try
                {
                    System.IO.File.Delete(filePath);
                }
                catch
                {
                    // Task is already deleted from the database.
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
