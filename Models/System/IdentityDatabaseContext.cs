using goynote.Models.Objects;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace goynote.Models.System
{
    public class IdentityDatabaseContext : IdentityDbContext<ApplicationUser>
    {
        public IdentityDatabaseContext(DbContextOptions<IdentityDatabaseContext> options) 
            : base(options)
        {
        }

        public DbSet<Note> Notes { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectSpecificNote> PSNotes { get; set; }
        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            mb.Entity<Note>()
                .HasOne(n => n.Workspace)
                .WithMany(w => w.Notes)
                .HasForeignKey(n => n.WorkspaceID)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<Project>()
                .HasOne(p => p.Workspace)
                .WithMany(w => w.Projects)
                .HasForeignKey(p => p.WorkspaceID)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<ProjectSpecificNote>()
                .HasOne(psn => psn.Project)
                .WithMany(p => p.PSNotes)
                .HasForeignKey(psn => psn.ProjectID)
                .OnDelete(DeleteBehavior.NoAction);

            mb.Entity<Attachment>()
                .HasOne(n => n.Note)
                .WithMany(n => n.Attachments)
                .HasForeignKey(n => n.NoteID)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
