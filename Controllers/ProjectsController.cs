using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using goynote.Models.Objects;
using goynote.Models.System;

namespace goynote.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly IdentityDatabaseContext _context;

        public ProjectsController(IdentityDatabaseContext context)
        {
            _context = context;
        }

        // GET: Project
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var projects = _context.Projects
                .Include(p => p.Workspace)
                .Where(p => p.Workspace.OwnerId == userId || 
                           p.Workspace.Members.Any(m => m.UserId == userId) || 
                           p.Workspace.IsPublic);
            
            return View(await projects.ToListAsync());
        }

        // GET: Project/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Workspace)
                    .ThenInclude(w => w.Members)
                .Include(p => p.PSNotes)
                    .ThenInclude(n => n.Attachments)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (project == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!CanAccessWorkspace(project.Workspace, userId))
            {
                return Forbid();
            }

            ViewData["CurrentUserId"] = userId;
            ViewData["IsOwner"] = project.Workspace.OwnerId == userId;
            return View(project);
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

            var project = new Project
            {
                WorkspaceID = workspaceId
            };

            return View(project);
        }

        // POST: Project/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,Description,WorkspaceID")] Project project)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var workspace = await _context.Workspaces
                .Include(w => w.Members)
                .FirstOrDefaultAsync(w => w.ID == project.WorkspaceID);
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
                project.ID = Guid.NewGuid();
                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Workspaces", new { id = project.WorkspaceID });
            }
            ViewData["WorkspaceID"] = new SelectList(_context.Workspaces, "ID", "Name", project.WorkspaceID);
            return View(project);
        }

        // GET: Project/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Workspace)
                    .ThenInclude(w => w.Members)
                .FirstOrDefaultAsync(p => p.ID == id);
            if (project == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!CanAccessWorkspace(project.Workspace, userId))
            {
                return Forbid();
            }

            ViewData["WorkspaceID"] = new SelectList(_context.Workspaces, "ID", "Name", project.WorkspaceID);
            return View(project);
        }

        // POST: Project/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("ID,Name,Description,WorkspaceID")] Project project)
        {
            if (id != project.ID)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var existingProject = await _context.Projects
                .Include(p => p.Workspace)
                    .ThenInclude(w => w.Members)
                .FirstOrDefaultAsync(p => p.ID == id);
            if (existingProject == null)
            {
                return NotFound();
            }

            if (!CanAccessWorkspace(existingProject.Workspace, userId))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingProject.Name = project.Name;
                    existingProject.Description = project.Description;
                    _context.Update(existingProject);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Workspaces", new { id = project.WorkspaceID });
            }
            ViewData["WorkspaceID"] = new SelectList(_context.Workspaces, "ID", "Name", project.WorkspaceID);
            return View(project);
        }

        // GET: Project/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Workspace)
                    .ThenInclude(w => w.Members)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (project == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!CanAccessWorkspace(project.Workspace, userId))
            {
                return Forbid();
            }

            return View(project);
        }

        // POST: Project/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var project = await _context.Projects
                .Include(p => p.Workspace)
                    .ThenInclude(w => w.Members)
                .FirstOrDefaultAsync(p => p.ID == id);
            
            if (project == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!CanAccessWorkspace(project.Workspace, userId))
            {
                return Forbid();
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(Guid id)
        {
            return _context.Projects.Any(e => e.ID == id);
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