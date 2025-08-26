#pragma warning disable CS8618

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DetailViewer.Core.Models
{
    public class Assembly
    {
        [Key]
        public int Id { get; set; }

        public int EskdNumberId { get; set; }

        public ESKDNumber EskdNumber { get; set; }

        public string Name { get; set; }

        public string Material { get; set; }

        public string Author { get; set; }

        public int? ParentAssemblyId { get; set; }

        [ForeignKey("ParentAssemblyId")]
        public virtual Assembly ParentAssembly { get; set; }

        public virtual ICollection<Assembly> SubAssemblies { get; set; } = new List<Assembly>();

        public virtual ICollection<AssemblyDetail> AssemblyDetails { get; set; } = new List<AssemblyDetail>();

        public virtual ICollection<ProductAssembly> ProductAssemblies { get; set; } = new List<ProductAssembly>();

        [Timestamp]
        public byte[] Version { get; set; }
    }
}
