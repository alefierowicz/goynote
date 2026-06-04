using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using goynote.Models.System;
using System.Security.Claims;

namespace goynote.ViewComponents
{
    // BASICALLY a dynamic view that pulls workspaces for the sidebar when logged in
    public class WorkspaceSidebarViewComponent : ViewComponent
    {
        private readonly IdentityDatabaseContext _context;

        public WorkspaceSidebarViewComponent(IdentityDatabaseContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userID = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userID == null)
                return Content(string.Empty);

            var workspaces = await _context.Workspaces
                                    .Where(w => w.OwnerId == userID || w.Members.Any(m => m.UserId == userID))
                                    .ToListAsync();

            Guid.TryParse(HttpContext.GetRouteValue("id")?.ToString(), out var currentId);
            ViewBag.CurrentWorkspaceId = currentId;

            return View(workspaces);
        }
    }
}
