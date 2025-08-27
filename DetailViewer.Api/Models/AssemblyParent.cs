#pragma warning disable CS8618

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DetailViewer.Api.Models
{
    public class AssemblyParent
    {
        /// <summary>
        /// Получает или устанавливает уникальный идентификатор связи родитель-потомок.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Получает или устанавливает идентификатор родительской сборки.
        /// </summary>
        public int ParentAssemblyId { get; set; }

        /// <summary>
        /// Получает или устанавливает навигационное свойство для родительской сборки.
        /// </summary>
        [ForeignKey("ParentAssemblyId")]
        public Assembly ParentAssembly { get; set; }

        /// <summary>
        /// Получает или устанавливает идентификатор дочерней сборки.
        /// </summary>
        public int ChildAssemblyId { get; set; }

        /// <summary>
        /// Получает или устанавливает навигационное свойство для дочерней сборки.
        /// </summary>
        [ForeignKey("ChildAssemblyId")]
        public Assembly ChildAssembly { get; set; }
    }
}
