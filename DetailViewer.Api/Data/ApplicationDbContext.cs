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

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ApplicationDbContext"/>.
        /// </summary>
        /// <param name="options">Параметры для данного контекста.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            //Database.EnsureCreated();
        }

        /// <summary>
        /// Конфигурирует модель, которая была обнаружена по соглашению из типов сущностей,
        /// представленных в свойствах <see cref="T:Microsoft.EntityFrameworkCore.DbSet`1" /> в этом контексте.
        /// </summary>
        /// <param name="modelBuilder">Построитель, используемый для создания модели для этого контекста.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DocumentDetailRecord>()
                .HasOne(d => d.EskdNumber)
                .WithMany()
                .HasForeignKey(d => d.EskdNumberId)
                .OnDelete(DeleteBehavior.Cascade);

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

            modelBuilder.Entity<AssemblyDetail>().HasKey(ad => new { ad.AssemblyId, ad.DetailId });

            modelBuilder.Entity<AssemblyDetail>()
                .HasOne(ad => ad.Assembly)
                .WithMany()
                .HasForeignKey(ad => ad.AssemblyId);

            modelBuilder.Entity<AssemblyDetail>()
                .HasOne(ad => ad.Detail)
                .WithMany()
                .HasForeignKey(ad => ad.DetailId);

            modelBuilder.Entity<AssemblyParent>().HasKey(ap => new { ap.ParentAssemblyId, ap.ChildAssemblyId });

            modelBuilder.Entity<AssemblyParent>()
                .HasOne(ap => ap.ParentAssembly)
                .WithMany()
                .HasForeignKey(ap => ap.ParentAssemblyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AssemblyParent>()
                .HasOne(ap => ap.ChildAssembly)
                .WithMany()
                .HasForeignKey(ap => ap.ChildAssemblyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}