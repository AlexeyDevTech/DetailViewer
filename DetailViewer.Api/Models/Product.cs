#pragma warning disable CS8618

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DetailViewer.Api.Models
{
    /// <summary>
    /// Сущность, представляющая изделие.
    /// </summary>
    public class Product
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор ЕСКД номера.
        /// </summary>
        public int EskdNumberId { get; set; }

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
        /// Автор изделия.
        /// </summary>
        public string Author { get; set; }

        [Timestamp]
        public byte[] Version { get; set; }
    }
}