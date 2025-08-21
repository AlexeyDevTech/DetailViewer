#pragma warning disable CS8618

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DetailViewer.Core.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        public int EskdNumberId { get; set; }

        public ESKDNumber EskdNumber { get; set; }

        public string Name { get; set; }

        public string Material { get; set; }

        public string Author { get; set; }

        public virtual ICollection<ProductAssembly> ProductAssemblies { get; set; } = new List<ProductAssembly>();

        [Timestamp]
        public byte[] Version { get; set; }
    }
}
