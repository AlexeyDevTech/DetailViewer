using DetailViewer.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DetailViewer.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<DocumentDetailRecord> DocumentRecords { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<ESKDNumber> ESKDNumbers { get; set; }
        public DbSet<Classifier> Classifiers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Assembly> Assemblies { get; set; }
        public DbSet<AssemblyDetail> AssemblyDetails { get; set; }
        public DbSet<ProductAssembly> ProductAssemblies { get; set; }
        public DbSet<AssemblyParent> AssemblyParents { get; set; }
        public DbSet<ProductDetail> ProductDetails { get; set; } // <-- НОВОЕ DbSet

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        { 
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DocumentDetailRecord>()
                .HasOne(d => d.EskdNumber)
                .WithMany()
                .HasForeignKey(d => d.EskdNumberId)
                .OnDelete(DeleteBehavior.Cascade);

            // Новая, полная конфигурация связи "многие-ко-многим" для AssemblyDetail
            modelBuilder.Entity<DocumentDetailRecord>()
                .HasMany(d => d.Assemblies)
                .WithMany(a => a.DocumentDetailRecords)
                .UsingEntity<AssemblyDetail>(
                    j => j
                        .HasOne(ad => ad.Assembly)
                        .WithMany()
                        .HasForeignKey(ad => ad.AssemblyId),
                    j => j
                        .HasOne(ad => ad.Detail)
                        .WithMany()
                        .HasForeignKey(ad => ad.DetailId),
                    j =>
                    {
                        j.HasKey(t => new { t.AssemblyId, t.DetailId });
                        j.ToTable("AssemblyDetails");
                    });

            // НОВАЯ конфигурация связи "многие-ко-многим" для ProductDetail
            modelBuilder.Entity<DocumentDetailRecord>()
                .HasMany(d => d.Products)
                .WithMany(p => p.DocumentDetailRecords)
                .UsingEntity<ProductDetail>(
                    j => j
                        .HasOne(pd => pd.Product)
                        .WithMany()
                        .HasForeignKey(pd => pd.ProductId),
                    j => j
                        .HasOne(pd => pd.Detail)
                        .WithMany()
                        .HasForeignKey(pd => pd.DetailId),
                    j =>
                    {
                        j.HasKey(t => new { t.ProductId, t.DetailId });
                        j.ToTable("ProductDetails"); // Указываем имя таблицы для связующей сущности
                    });

            modelBuilder.Entity<Classifier>()
                .HasMany(c => c.ESKDNumbers)
                .WithOne(e => e.ClassNumber)
                .HasForeignKey(e => e.ClassifierId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.EskdNumber)
                .WithMany()
                .HasForeignKey(p => p.EskdNumberId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Assembly>()
                .HasOne(a => a.EskdNumber)
                .WithMany()
                .HasForeignKey(a => a.EskdNumberId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductAssembly>().HasKey(pa => new { pa.ProductId, pa.AssemblyId });

            modelBuilder.Entity<ProductAssembly>()
                .HasOne(pa => pa.Product)
                .WithMany()
                .HasForeignKey(pa => pa.ProductId);

            modelBuilder.Entity<ProductAssembly>()
                .HasOne(pa => pa.Assembly)
                .WithMany()
                .HasForeignKey(pa => pa.AssemblyId);

            modelBuilder.Entity<AssemblyParent>().HasKey(ap => new { ap.ParentAssemblyId, ap.ChildAssemblyId });

            modelBuilder.Entity<AssemblyParent>()
                .HasOne(ap => ap.ParentAssembly)
                .WithMany()
                .HasForeignKey(ap => ap.ParentAssemblyId);

            modelBuilder.Entity<AssemblyParent>()
                .HasOne(ap => ap.ChildAssembly)
                .WithMany()
                .HasForeignKey(ap => ap.ChildAssemblyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}