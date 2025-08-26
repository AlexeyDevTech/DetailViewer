#pragma warning disable CS8618

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DetailViewer.Core.Models
{
    /// <summary>
    /// Представляет сборочную единицу (сборку).
    /// </summary>
    public class Assembly
    {
        /// <summary>
        /// Уникальный идентификатор сборки.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Внешний ключ для децимального номера.
        /// </summary>
        public int EskdNumberId { get; set; }

        /// <summary>
        /// Навигационное свойство для децимального номера.
        /// </summary>
        public ESKDNumber EskdNumber { get; set; }

        /// <summary>
        /// Наименование сборки.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Материал, из которого изготовлена сборка.
        /// </summary>
        public string Material { get; set; }

        /// <summary>
        /// Автор (разработчик) сборки.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Внешний ключ для родительской сборки (для иерархии).
        /// </summary>
        public int? ParentAssemblyId { get; set; }

        /// <summary>
        /// Навигационное свойство для родительской сборки.
        /// </summary>
        [ForeignKey("ParentAssemblyId")]
        public virtual Assembly ParentAssembly { get; set; }

        /// <summary>
        /// Коллекция дочерних сборок.
        /// </summary>
        public virtual ICollection<Assembly> SubAssemblies { get; set; } = new List<Assembly>();

        /// <summary>
        /// Коллекция связей с деталями, входящими в эту сборку.
        /// </summary>
        public virtual ICollection<AssemblyDetail> AssemblyDetails { get; set; } = new List<AssemblyDetail>();

        /// <summary>
        /// Коллекция связей с продуктами, в которые входит эта сборка.
        /// </summary>
        public virtual ICollection<ProductAssembly> ProductAssemblies { get; set; } = new List<ProductAssembly>();

        /// <summary>
        /// Версия строки для отслеживания оптимистичного параллелизма.
        /// </summary>
        [Timestamp]
        public byte[] Version { get; set; }
    }
}