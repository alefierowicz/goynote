using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using goynote.Models.System;

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
            if (!HttpContext.User.Identity?.IsAuthenticated ?? true)
                return Content(string.Empty);

            var workspaces = await _context.Workspaces.ToListAsync();

            Guid.TryParse(HttpContext.GetRouteValue("id")?.ToString(), out var currentId);
            ViewBag.CurrentWorkspaceId = currentId;

            return View(workspaces);
        }
    }
}
