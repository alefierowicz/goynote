using System;
using System.Collections.Generic;
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
    public class WorkspacesController : Controller
    {
        private readonly IdentityDatabaseContext _context;

        public WorkspacesController(IdentityDatabaseContext context)
        {
            _context = context;
        }

        // GET: Workspaces
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var userWorkspaces = await _context.Workspaces
                .Where(w => w.OwnerId == userId || w.Members.Any(m => m.UserId == userId) || w.IsPublic)
                .ToListAsync();

            return View(userWorkspaces);
        }

        // GET: Workspaces/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workspace = await _context.Workspaces
                .Include(w => w.Projects)
                .Include(w => w.Notes.Where(n => !(n is ProjectSpecificNote)))
                    .ThenInclude(n => n.Attachments)
                .Include(w => w.Members)
                .Include(w => w.Owner)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (workspace == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!CanAccessWorkspace(workspace, userId))
            {
                return Forbid();
            }

            ViewData["CurrentUserId"] = userId;
            ViewData["IsOwner"] = workspace.OwnerId == userId;
            return View(workspace);
        }

        // GET: Workspaces/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Workspaces/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,ID")] Workspace workspace)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                workspace.ID = Guid.NewGuid();
                workspace.OwnerId = userId;
                _context.Add(workspace);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }         
              
            return View(workspace);
        }

        // GET: Workspaces/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workspace = await _context.Workspaces.FindAsync(id);
            if (workspace == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (workspace.OwnerId != userId)
            {
                return Forbid();
            }

            return View(workspace);
        }

        // POST: Workspaces/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Name,Description,ID,IsPublic")] Workspace workspace)
        {
            if (id != workspace.ID)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingWorkspace = await _context.Workspaces.FindAsync(id);
                    if (existingWorkspace == null)
                    {
                        return NotFound();
                    }
                    
                    if (existingWorkspace.OwnerId != userId)
                    {
                        return Forbid();
                    }

                    existingWorkspace.Name = workspace.Name;
                    existingWorkspace.Description = workspace.Description;
                    existingWorkspace.IsPublic = workspace.IsPublic;
                    _context.Update(existingWorkspace);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WorkspaceExists(workspace.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(workspace);
        }

        // GET: Workspaces/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workspace = await _context.Workspaces
                .Include(w => w.Projects)
                .Include(n => n.Notes)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (workspace == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (workspace.OwnerId != userId)
            {
                return Forbid();
            }

            return View(workspace);
        }

        // POST: Workspaces/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var workspace = await _context.Workspaces.FindAsync(id);
            if (workspace == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (workspace.OwnerId != userId)
            {
                return Forbid();
            }

            _context.Workspaces.Remove(workspace);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Workspaces/Members/5
        public async Task<IActionResult> Members(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workspace = await _context.Workspaces
                .Include(w => w.Members)
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(w => w.ID == id);
            
            if (workspace == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (workspace.OwnerId != userId)
            {
                return Forbid();
            }

            return View(workspace);
        }

        // POST: Workspaces/AddMember
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(Guid workspaceId, string userEmail)
        {
            var workspace = await _context.Workspaces
                .Include(w => w.Members)
                .FirstOrDefaultAsync(w => w.ID == workspaceId);
            
            if (workspace == null)
            {
                TempData["ErrorMessage"] = "Workspace not found.";
                return RedirectToAction(nameof(Members), new { id = workspaceId });
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (workspace.OwnerId != userId)
            {
                TempData["ErrorMessage"] = "You don't have permission to add members.";
                return Forbid();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
            {
                TempData["ErrorMessage"] = "No user found with that email address.";
                return RedirectToAction(nameof(Members), new { id = workspaceId });
            }

            if (workspace.Members.Any(m => m.UserId == user.Id))
            {
                TempData["ErrorMessage"] = "This user is already a member of the workspace.";
                return RedirectToAction(nameof(Members), new { id = workspaceId });
            }

            var member = new WorkspaceMember
            {
                WorkspaceID = workspaceId,
                UserId = user.Id
            };

            _context.WorkspaceMembers.Add(member);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"User {user.Email} has been added to the workspace.";
            return RedirectToAction(nameof(Members), new { id = workspaceId });
        }

        // POST: Workspaces/RemoveMember
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(Guid workspaceId, Guid memberId)
        {
            var workspace = await _context.Workspaces.FindAsync(workspaceId);
            if (workspace == null)
            {
                TempData["ErrorMessage"] = "Workspace not found.";
                return RedirectToAction(nameof(Index));
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (workspace.OwnerId != userId)
            {
                TempData["ErrorMessage"] = "You don't have permission to remove members.";
                return Forbid();
            }

            var member = await _context.WorkspaceMembers.FindAsync(memberId);
            if (member == null)
            {
                TempData["ErrorMessage"] = "Member not found.";
                return RedirectToAction(nameof(Members), new { id = workspaceId });
            }

            _context.WorkspaceMembers.Remove(member);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Member has been removed from the workspace.";
            return RedirectToAction(nameof(Members), new { id = workspaceId });
        }

        private bool WorkspaceExists(Guid id)
        {
            return _context.Workspaces.Any(e => e.ID == id);
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
