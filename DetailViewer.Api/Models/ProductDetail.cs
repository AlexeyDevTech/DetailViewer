#pragma warning disable CS8618

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DetailViewer.Api.Models
{
    /// <summary>
    /// Представляет связующую сущность для отношения "многие-ко-многим" между Изделием и Деталью.
    /// </summary>
    public class ProductDetail
    {
        /// <summary>
        /// Внешний ключ для Изделия.
        /// </summary>
        public int ProductId { get; set; }
        /// <summary>
        /// Навигационное свойство для Изделия.
        /// </summary>
        public Product Product { get; set; }

        /// <summary>
        /// Внешний ключ для Детали.
        /// </summary>
        public int DetailId { get; set; }
        /// <summary>
        /// Навигационное свойство для Детали.
        /// </summary>
        public DocumentDetailRecord Detail { get; set; }
    }
}
