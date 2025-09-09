#pragma warning disable CS8618

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DetailViewer.Api.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public int EskdNumberId { get; set; }
        public ESKDNumber EskdNumber { get; set; }
        public string? Author { get; set; }

        /// <summary>
        /// Получает или устанавливает коллекцию деталей, входящих в это изделие.
        /// </summary>
        public ICollection<DocumentDetailRecord> DocumentDetailRecords { get; set; } = new List<DocumentDetailRecord>();
    }
}
