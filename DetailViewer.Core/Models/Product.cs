#pragma warning disable CS8618

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DetailViewer.Core.Models
{
    /// <summary>
    /// Представляет конечный продукт.
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Уникальный идентификатор продукта.
        /// </summary>
        [Key]
        public int Id { get; set; }

        public DateTime Date { get; set; }
        /// <summary>
        /// Внешний ключ для децимального номера продукта.
        /// </summary>
        public int EskdNumberId { get; set; }

        /// <summary>
        /// Навигационное свойство для децимального номера продукта.
        /// </summary>
        public ESKDNumber EskdNumber { get; set; }

        /// <summary>
        /// Наименование продукта.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Материал продукта.
        /// </summary>
        public string Material { get; set; }

        /// <summary>
        /// Автор (разработчик) продукта.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Коллекция связей сборок, входящих в этот продукт.
        /// </summary>
        public virtual ICollection<ProductAssembly> ProductAssemblies { get; set; } = new List<ProductAssembly>();

        /// <summary>
        /// Версия строки для отслеживания оптимистичного параллелизма.
        /// </summary>
        [Timestamp]
        public byte[] Version { get; set; }
    }
}