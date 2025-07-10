using DetailViewer.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace DetailViewer.Core.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<DocumentRecord> DocumentRecords { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<ESKDNumber> ESKDNumbers { get; set; }
        public DbSet<Classifier> Classifiers { get; set; }

        private readonly string _databasePath;

        public ApplicationDbContext()
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _databasePath = System.IO.Path.Join(folder, "detailviewer.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={_databasePath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ESKDNumber>()
                .HasOne(e => e.ClassNumber)
                .WithMany(c => c.ESKDNumbers)
                .HasForeignKey(e => e.ClassifierId);
        }
    }
}
