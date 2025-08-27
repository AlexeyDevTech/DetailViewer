#pragma warning disable CS8618

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DetailViewer.Api.Models
{
    public class Assembly
    {
        /// <summary>
        /// Получает или устанавливает уникальный идентификатор сборки.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Получает или устанавливает внешний ключ для номера ЕСКД.
        /// </summary>
        public int EskdNumberId { get; set; }

        /// <summary>
        /// Получает или устанавливает навигационное свойство для номера ЕСКД.
        /// </summary>
        public ESKDNumber EskdNumber { get; set; }

        /// <summary>
        /// Получает или устанавливает наименование сборки.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Получает или устанавливает материал сборки.
        /// </summary>
        public string? Material { get; set; }

        /// <summary>
        /// Получает или устанавливает автора сборки.
        /// </summary>
        public string? Author { get; set; }

        /// <summary>
        /// Получает или устанавливает версию записи для контроля параллелизма.
        /// </summary>
        [Timestamp]
        public byte[]? Version { get; set; }
    }
}