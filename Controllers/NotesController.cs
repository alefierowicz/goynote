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
        
        public NotesController(IdentityDatabaseContext context, S3Service S3Service, PDFService pdfService)
        {
            _context = context;
            _s3Service = S3Service;
            _pdfService = pdfService;
        }

        // GET: Note
        public async Task<IActionResult> Index()
        {
            var notes = _context.Notes.Include(p => p.Workspace);
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
                .Include(p => p.Attachments)
                .FirstOrDefaultAsync(m => m.ID == id);
                
            if (note == null)
            {
                return NotFound();
            }

            return View(note);
        }

        // GET: Project/Create
        public IActionResult Create(Guid workspaceId)
        {
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
            if (ModelState.IsValid)
            {
                note.ID = Guid.NewGuid(); // Assign a new GUID for the note ID
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
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return NotFound();

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
            var attachment = await _context.Attachments.FindAsync(id);
            if (attachment == null)
                return NotFound();
            
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
                .Include(p => p.Attachments)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (note == null)
                return NotFound();

            byte[] pdfBytes = _pdfService.GenerateNotePDF(note);
            string fileName = $"{note.Name.Replace(" ", "_")}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}