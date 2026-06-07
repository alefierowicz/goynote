using goynote.Models.System;
using Microsoft.AspNetCore.Mvc;
using goynote.Models.API;
using goynote.Models.Objects;
using Microsoft.EntityFrameworkCore;

namespace goynote.Controllers.API
{
    [ApiController]
    [Route("api/workspaces")]
    public class PublicWorkspacesController : Controller
    {
        private readonly IdentityDatabaseContext _context;
        public PublicWorkspacesController(IdentityDatabaseContext c) => _context = c;

        // GET /api/workspaces
        public async Task<ActionResult<IEnumerable<WorkspaceDTO>>> GetPublic()
        {
            var workspaces = await _context.Workspaces
                .Where(w => w.IsPublic)
                .Include(w => w.Owner)
                .Include(w => w.Notes).ThenInclude(n => n.Attachments)
                .ToListAsync();

            var DTOs = workspaces.Select(MapToDTO).ToList();
            return Ok(DTOs);
        }

        private static WorkspaceDTO MapToDTO(Workspace w)
        {
            var noteDTOs = w.Notes
                .Where(n => !(n is ProjectSpecificNote))
                .Select(n => new NoteDTO(
                    n.ID,
                    n.Name,
                    n.Content,
                    (n.Attachments ?? new List<Attachment>())
                    .Select(a => new AttachmentDTO(
                        a.ID, a.FileName, a.ContentType, a.FileSize, a.CreatedAt
                    )).ToList()
                )).ToList();
            return new WorkspaceDTO(w.ID, w.Name, w.Description, w.Owner.Email!, noteDTOs);
        }
    }

}