
using System;
using System.ComponentModel.DataAnnotations;

namespace DetailViewer.Core.Models
{
    public enum OperationType
    {
        Create,
        Update,
        Delete
    }

    public class ChangeLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string EntityName { get; set; }

        [Required]
        public string EntityId { get; set; }

        [Required]
        public OperationType OperationType { get; set; }

        public string? Payload { get; set; } // JSON representation of the changed data

        [Required]
        public DateTime Timestamp { get; set; }
    }
}
