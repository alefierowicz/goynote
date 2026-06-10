using goynote.Models.System;
using Microsoft.AspNetCore.Mvc;
using goynote.Models.API;
using goynote.Models.Objects;
using Microsoft.EntityFrameworkCore;

namespace goynote.Controllers.API
{
    [ApiController]
    [Route("api/workspaces")]
    public class PublicWorkspacesController : ControllerBase
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
                .Include(w => w.Projects).ThenInclude(p => p.PSNotes).ThenInclude(n => n.Attachments)
                .AsSplitQuery()
                .ToListAsync();

            var DTOs = workspaces.Select(MapWorkspace).ToList();
            return Ok(DTOs);
        }

        // GET /api/workspaces/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<WorkspaceDTO>> GetPublicByID(Guid id)
        {
            var workspace = await _context.Workspaces
                .Include(w => w.Owner)
                .Include(w => w.Notes).ThenInclude(n => n.Attachments)
                .Include(w => w.Projects).ThenInclude(p => p.PSNotes).ThenInclude(n => n.Attachments)
                .AsSplitQuery()
                .FirstOrDefaultAsync(w => w.ID == id && w.IsPublic);
            if (workspace == null) return NotFound();
            return Ok(MapWorkspace(workspace));
        }
        private static WorkspaceDTO MapWorkspace(Workspace w)
        {
            var noteDTOs = w.Notes
                .Where(n => !(n is ProjectSpecificNote))
                .Select(MapNote)
                .ToList();

            var projectDTOs = w.Projects
                .Select(p => new ProjectDTO(
                    p.ID,
                    p.Name,
                    p.Description,
                    p.PSNotes.Select(MapNote).ToList()
                )).ToList();

            return new WorkspaceDTO(w.ID, w.Name, w.Description, w.Owner.Email!, noteDTOs, projectDTOs);
        }

        private static NoteDTO MapNote(Note n) =>
            new NoteDTO(
                n.ID,
                n.Name,
                n.Content,
                (n.Attachments ?? new List<Attachment>())
                    .Select(a => new AttachmentDTO(
                        a.ID, a.FileName, a.ContentType, a.FileSize, a.CreatedAt
                    )).ToList()
            );
    }

}