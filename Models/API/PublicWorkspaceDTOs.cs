namespace goynote.Models.API
{
    public record AttachmentDTO(
        Guid Id,
        string FileName,
        string ContentType,
        int FileSize,
        DateTime CreatedAt
    );
    public record NoteDTO(
        Guid Id,
        string Name,
        string Content,
        IEnumerable<AttachmentDTO> Attachments
    );

    public record WorkspaceDTO(
        Guid Id,
        string Name,
        string? Description,
        string OwnerEmail, IEnumerable<NoteDTO> Notes
    );
}