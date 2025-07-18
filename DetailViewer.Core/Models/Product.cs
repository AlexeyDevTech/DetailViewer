
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DetailViewer.Core.Models
{
    /// <summary>
    /// Сущность, представляющая изделие.
    /// </summary>
    public class Product
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// ЕСКД номер изделия.
        /// </summary>
        public ESKDNumber EskdNumber { get; set; }

        /// <summary>
        /// Наименование изделия.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Материал изделия.
        /// </summary>
        public string Material { get; set; }

        /// <summary>
        /// Коллекция связей со сборками, входящими в состав изделия.
        /// </summary>
        public virtual ICollection<ProductAssembly> ProductAssemblies { get; set; } = new List<ProductAssembly>();
    }
}
