
#pragma warning disable CS8618

using System;
using System.ComponentModel.DataAnnotations;

namespace DetailViewer.Api.Models
{
    public class ConflictLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string EntityName { get; set; }

        [Required]
        public string EntityId { get; set; }

        [Required]
        public string LocalPayload { get; set; }

        [Required]
        public string RemotePayload { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }
    }
}
