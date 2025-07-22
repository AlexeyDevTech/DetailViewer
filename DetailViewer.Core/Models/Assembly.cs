
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DetailViewer.Core.Models
{
    /// <summary>
    /// Сущность, представляющая сборку.
    /// </summary>
    public class Assembly
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор ЕСКД номера.
        /// </summary>
        public int EskdNumberId { get; set; }

        /// <summary>
        /// ЕСКД номер сборки.
        /// </summary>
        public ESKDNumber EskdNumber { get; set; }

        /// <summary>
        /// Наименование сборки.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Материал сборки.
        /// </summary>
        public string Material { get; set; }

        /// <summary>
        /// Автор сборки.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Идентификатор родительской сборки (для вложенности).
        /// </summary>
        public int? ParentAssemblyId { get; set; }

        /// <summary>
        /// Родительская сборка.
        /// </summary>
        [ForeignKey("ParentAssemblyId")]
        public virtual Assembly ParentAssembly { get; set; }

        /// <summary>
        /// Коллекция дочерних сборок.
        /// </summary>
        public virtual ICollection<Assembly> SubAssemblies { get; set; } = new List<Assembly>();

        /// <summary>
        /// Коллекция связей с деталями, входящими в состав сборки.
        /// </summary>
        public virtual ICollection<AssemblyDetail> AssemblyDetails { get; set; } = new List<AssemblyDetail>();

        /// <summary>
        /// Коллекция связей с изделиями, к которым относится сборка.
        /// </summary>
        public virtual ICollection<ProductAssembly> ProductAssemblies { get; set; } = new List<ProductAssembly>();
    }
}
