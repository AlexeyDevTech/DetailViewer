#pragma warning disable CS8618

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DetailViewer.Api.Models
{
    public class Product
    {
        /// <summary>
        /// Получает или устанавливает уникальный идентификатор продукта.
        /// </summary>
        [Key]
        public int Id { get; set; }


        public DateTime Date { get; set; }
        /// <summary>
        /// Получает или устанавливает внешний ключ для номера ЕСКД.
        /// </summary>
        public int EskdNumberId { get; set; }

        /// <summary>
        /// Получает или устанавливает навигационное свойство для номера ЕСКД.
        /// </summary>
        public ESKDNumber EskdNumber { get; set; }

        /// <summary>
        /// Получает или устанавливает наименование продукта.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Получает или устанавливает материал продукта.
        /// </summary>
        public string? Material { get; set; }

        /// <summary>
        /// Получает или устанавливает автора продукта.
        /// </summary>
        public string? Author { get; set; }

        /// <summary>
        /// Получает или устанавливает версию записи для контроля параллелизма.
        /// </summary>
        [Timestamp]
        public byte[]? Version { get; set; }
    }
}