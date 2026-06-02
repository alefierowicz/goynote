using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using goynote.Models.Objects;
using goynote.Models.System;

namespace goynote.Controllers
{
    [Authorize]
    public class NotesController : Controller
    {
        const long MAX_FILE_SIZE = 10 * 1024 * 1024;
        private readonly IdentityDatabaseContext _context;
        private readonly S3Service _s3Service;
        private readonly PDFService _pdfService;
        private readonly MarkdownService _markdownService;
        
        public NotesController(IdentityDatabaseContext context, S3Service S3Service, PDFService pdfService, MarkdownService markdownService)
        {
            _context = context;
            _s3Service = S3Service;
            _pdfService = pdfService;
            _markdownService = markdownService;
        }

        // GET: Note
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var notes = _context.Notes
                .Include(p => p.Workspace)
                .Where(n => n.Workspace.OwnerId == userId || 
                           n.Workspace.Members.Any(m => m.UserId == userId) || 
                           n.Workspace.IsPublic);
            
            return View(await notes.ToListAsync());
        }

        // GET: Note/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var note = await _context.Notes
                .Include(p => p.Workspace)
                    .ThenInclude(w => w.Members)
                .Include(p => p.Attachments)
                .FirstOrDefaultAsync(m => m.ID == id);
                
            if (note == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!CanAccessWorkspace(note.Workspace, userId))
            {
                return Forbid();
            }

            ViewData["CurrentUserId"] = userId;
            ViewData["IsOwner"] = note.Workspace.OwnerId == userId;
            return View(note);
        }

        // GET: Project/Create
        public async Task<IActionResult> Create(Guid workspaceId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var workspace = await _context.Workspaces
                .Include(w => w.Members)
                .FirstOrDefaultAsync(w => w.ID == workspaceId);
            if (workspace == null)
            {
                return NotFound();
            }

            if (workspace.OwnerId != userId && !workspace.Members.Any(m => m.UserId == userId))
            {
                return Forbid();
            }

            var note = new Note
            {
                WorkspaceID = workspaceId
            };

            return View(note);
        }

        // POST: Project/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,Content,WorkspaceID")] Note note, IFormFile? file)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var workspace = await _context.Workspaces
                .Include(w => w.Members)
                .FirstOrDefaultAsync(w => w.ID == note.WorkspaceID);
            if (workspace == null)
            {
                return NotFound();
            }

            if (workspace.OwnerId != userId && !workspace.Members.Any(m => m.UserId == userId))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                note.ID = Guid.NewGuid();
                _context.Add(note);
                if(file != null)
                {
                    if(file.Length > MAX_FILE_SIZE)
                    {
                        ModelState.AddModelError("", "Uploaded file was too big.");
                        return View(note);
                    }

                    string? key = await _s3Service.UploadAsync(file);
                    if (key == null) {
                        ModelState.AddModelError("", "File upload failed - note was not saved.");
                        return View(note);
                    }

                    _context.Attachments.Add(new Attachment
                        {
                            NoteID = note.ID,
                            Note = note,
                            S3Key = key,
                            FileName = file.FileName,
                            ContentType = file.ContentType,
                            FileSize = (int)file.Length, 
                        }
                    );
                }
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Workspaces", new { id = note.WorkspaceID });
            }
            ViewData["WorkspaceID"] = new SelectList(_context.Workspaces, "ID", "Name", note.WorkspaceID);
            return View(note);
        }

        // GET: Notes/CreateProjectNote?projectId=(...)
        public async Task<IActionResult> CreateProjectNote(Guid projectId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var project = await _context.Projects
                .Include(p => p.Workspace)
                    .ThenInclude(w => w.Members)
                .FirstOrDefaultAsync(p => p.ID == projectId);
            if (project == null) 
                return NotFound();

            if (project.Workspace.OwnerId != userId && !project.Workspace.Members.Any(m => m.UserId == userId))
            {
                return Forbid();
            }

            ProjectSpecificNote psn = new ProjectSpecificNote
            {
                ProjectID = projectId,
                WorkspaceID = project.WorkspaceID
            };

            return View(psn);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProjectNote([Bind("Name,Content,ProjectID,WorkspaceID")] ProjectSpecificNote psn, IFormFile? file)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var project = await _context.Projects
                .Include(p => p.Workspace)
                    .ThenInclude(w => w.Members)
                .FirstOrDefaultAsync(p => p.ID == psn.ProjectID);
            if (project == null)
                return NotFound();

            if (project.Workspace.OwnerId != userId && !project.Workspace.Members.Any(m => m.UserId == userId))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                psn.ID = Guid.NewGuid();
                _context.Add(psn);

                if (file != null)
                {
                    if (file.Length > MAX_FILE_SIZE)
                    {
                        ModelState.AddModelError("", "Uploaded file was too big (Max accepted size: 10 MB)");
                        return View(psn);
                    }

                    var key = await _s3Service.UploadAsync(file);
                    if (key == null)
                    {
                        ModelState.AddModelError("", "File upload failed. Note was not saved.");
                        return View(psn);
                    }

                    _context.Add(new Attachment
                    {
                        ID = Guid.NewGuid(),
                        NoteID = psn.ID,
                        S3Key = key,
                        FileName = file.FileName,
                        ContentType = file.ContentType,
                        FileSize = (int)file.Length
                    });
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Projects", new { id = psn.ProjectID });
            }
            return View(psn);
        }

        // GET: Notes/Download/{id}
        public async Task<IActionResult> Download(Guid id)
        {
            var attachment = await _context.Attachments
                .Include(a => a.Note)
                .ThenInclude(n => n.Workspace)
                    .ThenInclude(w => w.Members)
                .FirstOrDefaultAsync(a => a.ID == id);
            if (attachment == null)
                return NotFound();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!CanAccessWorkspace(attachment.Note.Workspace, userId))
            {
                return Forbid();
            }
            
            string? url = await _s3Service.GenerateURLAsync(attachment.S3Key, attachment.FileName);
            if (url == null)
            {
                return Problem("Could not generate pre-signed URL");
            }
            return Redirect(url);
        }

        public async Task<IActionResult> DownloadPDF(Guid id)
        {
            var note = await _context.Notes
                .Include(p => p.Workspace)
                    .ThenInclude(w => w.Members)
                .Include(p => p.Attachments)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (note == null)
                return NotFound();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!CanAccessWorkspace(note.Workspace, userId))
            {
                return Forbid();
            }

            byte[] pdfBytes = _pdfService.GenerateNotePDF(note);
            string fileName = $"{note.Name.Replace(" ", "_")}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

        public async Task<IActionResult> DownloadMarkdown(Guid id)
        {
            var note = await _context.Notes
                .Include(p => p.Workspace)
                    .ThenInclude(w => w.Members)
                .Include(p => p.Attachments)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (note == null)
                return NotFound();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!CanAccessWorkspace(note.Workspace, userId))
            {
                return Forbid();
            }

            byte[] markdownBytes = _markdownService.GenerateNoteMarkdown(note);
            string fileName = $"{note.Name.Replace(" ", "_")}.md";

            return File(markdownBytes, "text/markdown", fileName);
        }

        // GET: Notes/Edit/{id}
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var note = await _context.Notes
                .Include(n => n.Workspace)
                    .ThenInclude(w => w.Members)
                .FirstOrDefaultAsync(n => n.ID == id);
            if (note == null) return NotFound();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!CanAccessWorkspace(note.Workspace, userId))
            {
                return Forbid();
            }

            return View(note);
        }

        // POST: Notes/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("ID,Name,Content,WorkspaceID")] Note note)
        {
            if (id != note.ID) return NotFound();

            if (!ModelState.IsValid) return View(note);

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var existing = await _context.Notes
                .Include(n => n.Workspace)
                    .ThenInclude(w => w.Members)
                .FirstOrDefaultAsync(n => n.ID == id);
            if (existing == null) return NotFound();

            if (!CanAccessWorkspace(existing.Workspace, userId))
            {
                return Forbid();
            }

            existing.Name = note.Name;
            existing.Content = note.Content;

            await _context.SaveChangesAsync();

            if (existing is ProjectSpecificNote psn)
                return RedirectToAction("Details", "Projects", new { id = psn.ProjectID });

            return RedirectToAction("Details", "Workspaces", new { id = existing.WorkspaceID });
        }

        // GET: Notes/Delete/{id}
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var note = await _context.Notes
                .Include(n => n.Workspace)
                    .ThenInclude(w => w.Members)
                .Include(n => n.Attachments)
                .FirstOrDefaultAsync(n => n.ID == id);

            if (note == null) return NotFound();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!CanAccessWorkspace(note.Workspace, userId))
            {
                return Forbid();
            }

            return View(note);
        }

        // POST: Notes/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var note = await _context.Notes
                .Include(n => n.Workspace)
                    .ThenInclude(w => w.Members)
                .FirstOrDefaultAsync(n => n.ID == id);
            if (note == null) return NotFound();

            if (!CanAccessWorkspace(note.Workspace, userId))
            {
                return Forbid();
            }

            Guid workspaceId = note.WorkspaceID;
            Guid? projectId = (note as ProjectSpecificNote)?.ProjectID;

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            if (projectId.HasValue)
                return RedirectToAction("Details", "Projects", new { id = projectId.Value });

            return RedirectToAction("Details", "Workspaces", new { id = workspaceId });
        }

        private bool CanAccessWorkspace(Workspace workspace, string? userId)
        {
            if (userId == null)
                return false;

            return workspace.OwnerId == userId || 
                   workspace.IsPublic || 
                   workspace.Members.Any(m => m.UserId == userId);
        }
    }
}