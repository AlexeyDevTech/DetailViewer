#pragma warning disable CS8618

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DetailViewer.Api.Models
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

        [Timestamp]
        public byte[] Version { get; set; }
    }
}