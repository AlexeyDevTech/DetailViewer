#pragma warning disable CS8618

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DetailViewer.Api.Models
{
    public class Assembly
    {
        /// <summary>
        /// Уникальный идентификатор сборки
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Наименование сборки (может быть не задано)
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        public string? Author { get; set; } // <-- ВОССТАНОВЛЕННОЕ СВОЙСТВО

        /// <summary>
        /// Внешний ключ для номера ЕСКД
        /// </summary>
        public int EskdNumberId { get; set; }

        /// <summary>
        /// Навигационное свойство для номера ЕСКД
        /// </summary>
        public ESKDNumber EskdNumber { get; set; }

        /// <summary>
        /// Коллекция деталей, входящих в эту сборку.
        /// </summary>
        public ICollection<DocumentDetailRecord> DocumentDetailRecords { get; set; } = new List<DocumentDetailRecord>();
    }
}
