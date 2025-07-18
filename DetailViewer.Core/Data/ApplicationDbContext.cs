using DetailViewer.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DetailViewer.Core.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<DocumentDetailRecord> DocumentRecords { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<ESKDNumber> ESKDNumbers { get; set; }
        public DbSet<Classifier> Classifiers { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DocumentDetailRecord>()
                .HasOne(d => d.ESKDNumber)
                .WithMany()
                .HasForeignKey(d => d.ESKDNumberId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ESKDNumber>()
                .HasOne(e => e.ClassNumber)
                .WithMany(c => c.ESKDNumbers)
                .HasForeignKey(e => e.ClassifierId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}