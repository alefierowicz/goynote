using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using goynote.Models.Objects;
using goynote.Models.System;

namespace goynote.Controllers
{
    public class NotesController : Controller
    {
        private readonly DatabaseContext _context;

        public NotesController(DatabaseContext context)
        {
            _context = context;
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
        public async Task<IActionResult> Create([Bind("ID,Name,Content,WorkspaceID")] Note note)
        {
            if (ModelState.IsValid)
            {
                note.ID = Guid.NewGuid(); // Assign a new GUID for the note ID
                _context.Add(note);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Workspaces", new { id = note.WorkspaceID });
            }
            ViewData["WorkspaceID"] = new SelectList(_context.Workspaces, "ID", "Name", note.WorkspaceID);
            return View(note);
        }

        // // GET: Project/Edit/5
        // public async Task<IActionResult> Edit(Guid? id)
        // {
        //     if (id == null)
        //     {
        //         return NotFound();
        //     }

        //     var project = await _context.Projects.FindAsync(id);
        //     if (project == null)
        //     {
        //         return NotFound();
        //     }
        //     ViewData["WorkspaceID"] = new SelectList(_context.Workspaces, "ID", "Name", project.WorkspaceID);
        //     return View(project);
        // }

        // // POST: Project/Edit/5
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Edit(Guid id, [Bind("ID,Name,Description,WorkspaceID")] Project project)
        // {
        //     if (id != project.ID)
        //     {
        //         return NotFound();
        //     }

        //     if (ModelState.IsValid)
        //     {
        //         try
        //         {
        //             _context.Update(project);
        //             await _context.SaveChangesAsync();
        //         }
        //         catch (DbUpdateConcurrencyException)
        //         {
        //             if (!ProjectExists(project.ID))
        //             {
        //                 return NotFound();
        //             }
        //             else
        //             {
        //                 throw;
        //             }
        //         }
        //         return RedirectToAction(nameof(Index));
        //     }
        //     ViewData["WorkspaceID"] = new SelectList(_context.Workspaces, "ID", "Name", project.WorkspaceID);
        //     return View(project);
        // }

        // // GET: Project/Delete/5
        // public async Task<IActionResult> Delete(Guid? id)
        // {
        //     if (id == null)
        //     {
        //         return NotFound();
        //     }

        //     var project = await _context.Projects
        //         .Include(p => p.Workspace)
        //         .FirstOrDefaultAsync(m => m.ID == id);
        //     if (project == null)
        //     {
        //         return NotFound();
        //     }

        //     return View(project);
        // }

        // // POST: Project/Delete/5
        // [HttpPost, ActionName("Delete")]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> DeleteConfirmed(Guid id)
        // {
        //     var project = await _context.Projects.FindAsync(id);
        //     if (project != null)
        //     {
        //         _context.Projects.Remove(project);
        //     }

        //     await _context.SaveChangesAsync();
        //     return RedirectToAction(nameof(Index));
        // }

        // private bool ProjectExists(Guid id)
        // {
        //     return _context.Projects.Any(e => e.ID == id);
        // }
    }
}