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
        [HttpGet]
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

        // GET /api/workspaces/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<IEnumerable<WorkspaceDTO>>> GetPublicByID(Guid id)
        {
            var workspace = await _context.Workspaces
                .Include(w => w.Owner)
                .Include(w => w.Notes)
                .ThenInclude(n => n.Attachments)
                .FirstOrDefaultAsync(w => w.ID == id && w.IsPublic);
            if (workspace == null) return NotFound();
            return Ok(MapToDTO(workspace));
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