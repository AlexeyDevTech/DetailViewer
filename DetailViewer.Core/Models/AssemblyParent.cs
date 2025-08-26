using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DetailViewer.Core.Models
{
    /// <summary>
    /// Представляет иерархическую связь "многие-ко-многим" между родительской и дочерней сборками.
    /// </summary>
    public class AssemblyParent
    {
        /// <summary>
        /// Уникальный идентификатор связи.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Внешний ключ для родительской сборки.
        /// </summary>
        public int ParentAssemblyId { get; set; }

        /// <summary>
        /// Навигационное свойство к родительской сборке.
        /// </summary>
        [ForeignKey("ParentAssemblyId")]
        public Assembly ParentAssembly { get; set; }

        /// <summary>
        /// Внешний ключ для дочерней сборки.
        /// </summary>
        public int ChildAssemblyId { get; set; }

        /// <summary>
        /// Навигационное свойство к дочерней сборке.
        /// </summary>
        [ForeignKey("ChildAssemblyId")]
        public Assembly ChildAssembly { get; set; }
    }
}